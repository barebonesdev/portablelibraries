using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ToolsPortable;
using System.Collections;
using BareMvvm.Core;
using System.Diagnostics;
using System.Reflection;

namespace BareMvvm.Core.Binding
{
    public class BindingHost
    {
        private PropertyChangedEventHandler _dataContextPropertyChangedHandler;
        private object _dataContext;
        /// <summary>
        /// The DataContext for binding
        /// </summary>
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                if (value == _dataContext)
                {
                    return;
                }

                // Unregister old
                if (_dataContext is INotifyPropertyChanged && _dataContextPropertyChangedHandler != null)
                {
                    (_dataContext as INotifyPropertyChanged).PropertyChanged -= _dataContextPropertyChangedHandler;
                }

                _dataContext = value;

                // Register new
                if (value is INotifyPropertyChanged)
                {
                    if (_dataContextPropertyChangedHandler == null)
                    {
                        _dataContextPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(DataContext_PropertyChanged).Handler;
                    }
                    (value as INotifyPropertyChanged).PropertyChanged += _dataContextPropertyChangedHandler;
                }

                UpdateAllBindings();
            }
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We call ToArray() since a binding action could cause a new binding to be added while we're working
            Action[] existingActions = null;
            if (_bindings.TryGetValue(e.PropertyName, out List<Action> actions))
            {
                existingActions = actions.ToArray();
            }

            _subPropertyBindings.TryGetValue(e.PropertyName, out BindingHost existingSubBinding);

            if (existingActions != null)
            {
                ExecuteActions(existingActions);
            }

            if (existingSubBinding != null)
            {
                existingSubBinding.DataContext = GetValueFromSingleProperty(e.PropertyName);
            }
        }

        private void UpdateAllBindings()
        {
            // Grab these as a copied array so that any of the sub actions don't modify them as we iterate
            Action[] existingActions = _bindings.Values.SelectMany(i => i).ToArray();
            KeyValuePair<string, BindingHost>[] existingSubBindings = _subPropertyBindings.ToArray();

            ExecuteActions(existingActions);

            foreach (var subBinding in existingSubBindings)
            {
                subBinding.Value.DataContext = GetValueFromSingleProperty(subBinding.Key);
            }
        }

        private void ExecuteActions(Action[] actions)
        {
            foreach (var a in actions)
            {
                try
                {
                    a.Invoke();
                }
                catch
#if DEBUG
                (Exception ex)
#endif
                {
#if DEBUG
                    Debug.WriteLine("Failed to update binding: " + ex);
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
#endif
                }
            }
        }

        private Dictionary<string, List<Action>> _bindings = new Dictionary<string, List<Action>>();
        private Dictionary<string, BindingHost> _subPropertyBindings = new Dictionary<string, BindingHost>();

        public void SetBinding<T>(string propertyPath, Action<T> action)
        {
            SetBinding(propertyPath, () =>
            {
                object value = GetValue(propertyPath);
                if (value == null)
                {
                    action(default(T));
                }
                else
                {
                    action((T)value);
                }
            });
        }

        public void SetBinding(string propertyPath, Action<object> action)
        {
            SetBinding(propertyPath, () =>
            {
                object value = GetValue(propertyPath);
                action(value);
            });
        }

        public void SetBinding(string propertyPath, Action action, bool skipInvokingActionImmediately = false)
        {
            SetBinding(propertyPath.Split('.'), action, skipInvokingActionImmediately);
        }

        private void SetBinding(string[] propertyPaths, Action action, bool skipInvokingActionImmediately)
        {
            string immediatePath = propertyPaths[0];

            if (propertyPaths.Length == 1)
            {
                List<Action> storedActions;
                if (!_bindings.TryGetValue(immediatePath, out storedActions))
                {
                    storedActions = new List<Action>();
                    _bindings[immediatePath] = storedActions;
                }

                storedActions.Add(action);

                // We require DataContext to be set here since bindings can be wired before DataContext is set
                if (DataContext != null && !skipInvokingActionImmediately)
                {
                    action();
                }
            }
            else
            {
                BindingHost subBinding;
                if (!_subPropertyBindings.TryGetValue(immediatePath, out subBinding))
                {
                    subBinding = new BindingHost()
                    {
                        DataContext = GetValueFromSingleProperty(propertyPaths[0])
                    };
                    _subPropertyBindings[immediatePath] = subBinding;
                }

                subBinding.SetBinding(propertyPaths.Skip(1).ToArray(), action, skipInvokingActionImmediately);

                // For this we need to execute first time even if data context was null (for example binding Class.Name should execute even if Class was null)
                if (DataContext != null && subBinding.DataContext == null && !skipInvokingActionImmediately)
                {
                    action();
                }
            }
        }

        private object GetValueFromSingleProperty(string propertyName)
        {
            return DataContext?.GetType().GetProperty(propertyName).GetValue(DataContext);
        }

        private object GetValue(string propertyPath)
        {
            string[] paths = propertyPath.Split('.');

            object obj = DataContext;
            foreach (var propertyName in paths)
            {
                if (obj == null)
                {
                    return null;
                }

                obj = obj.GetType().GetProperty(propertyName).GetValue(obj);
            }

            return obj;
        }

        public Tuple<object, PropertyInfo> GetProperty(string propertyPath)
        {
            string[] paths = propertyPath.Split('.');

            object obj = DataContext;
            foreach (var propertyName in paths.Take(paths.Length - 1))
            {
                if (obj == null)
                {
                    return null;
                }

                obj = obj.GetType().GetProperty(propertyName).GetValue(obj);
            }

            if (obj == null)
            {
                return null;
            }

            return new Tuple<object, PropertyInfo>(obj, obj.GetType().GetProperty(paths.Last()));
        }

        protected void SetValue(string propertyPath, object value)
        {
            var property = GetProperty(propertyPath);
            if (property != null)
            {
                property.Item2.SetValue(property.Item1, value);
            }
        }

        public void SetBindings(string[] propertyPaths, Action action)
        {
            foreach (var p in propertyPaths)
            {
                SetBinding(p, action, skipInvokingActionImmediately: true);
            }

            if (DataContext != null)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Unregisters all handlers and everything and sets DataContext to null.
        /// </summary>
        public void Unregister()
        {
            _bindings.Clear();

            foreach (var subBinding in _subPropertyBindings.Values)
            {
                subBinding.Unregister();
            }

            _subPropertyBindings.Clear();

            DataContext = null;
        }
    }
}