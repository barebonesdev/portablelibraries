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
            bool wasResultOfSettingValue = _immediatePropertyChangeNotificationsToSkipOnce.Remove(e.PropertyName);

            // We call ToArray() since a binding action could cause a new binding to be added while we're working
            Action[] existingActions = null;
            if (_bindings.TryGetValue(e.PropertyName, out List<BindingRegistration> bindings))
            {
                if (wasResultOfSettingValue)
                {
                    existingActions = bindings.Where(i => i.TriggerEvenWhenSetThroughBinding).Select(i => i.Action).ToArray();
                }
                else
                {
                    existingActions = bindings.Select(i => i.Action).ToArray();
                }
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
            Action[] existingActions = _bindings.Values.SelectMany(i => i).Select(i => i.Action).ToArray();
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

        private class BindingRegistration
        {
            public Action Action { get; private set; }

            public bool TriggerEvenWhenSetThroughBinding { get; private set; }

            public BindingRegistration(Action action, bool triggerEvenWhenSetThroughBinding)
            {
                Action = action;
                TriggerEvenWhenSetThroughBinding = triggerEvenWhenSetThroughBinding;
            }
        }

        private Dictionary<string, List<BindingRegistration>> _bindings = new Dictionary<string, List<BindingRegistration>>();
        private Dictionary<string, BindingHost> _subPropertyBindings = new Dictionary<string, BindingHost>();

        public void SetBinding<T>(string propertyPath, Action<T> action, bool triggerEvenWhenSetThroughBinding = false)
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
            }, triggerEvenWhenSetThroughBinding: triggerEvenWhenSetThroughBinding);
        }

        public void SetBinding(string propertyPath, Action<object> action, bool triggerEvenWhenSetThroughBinding = false)
        {
            SetBinding(propertyPath, () =>
            {
                object value = GetValue(propertyPath);
                action(value);
            }, triggerEvenWhenSetThroughBinding: triggerEvenWhenSetThroughBinding);
        }

        public void SetBinding(string propertyPath, Action action, bool skipInvokingActionImmediately = false, bool triggerEvenWhenSetThroughBinding = false)
        {
            SetBinding(propertyPath.Split('.'), action, skipInvokingActionImmediately, triggerEvenWhenSetThroughBinding: triggerEvenWhenSetThroughBinding);
        }

        private void SetBinding(string[] propertyPaths, Action action, bool skipInvokingActionImmediately, bool triggerEvenWhenSetThroughBinding)
        {
            string immediatePath = propertyPaths[0];

            if (propertyPaths.Length == 1)
            {
                List<BindingRegistration> storedBindings;
                if (!_bindings.TryGetValue(immediatePath, out storedBindings))
                {
                    storedBindings = new List<BindingRegistration>();
                    _bindings[immediatePath] = storedBindings;
                }

                storedBindings.Add(new BindingRegistration(action, triggerEvenWhenSetThroughBinding));

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

                subBinding.SetBinding(propertyPaths.Skip(1).ToArray(), action, skipInvokingActionImmediately, triggerEvenWhenSetThroughBinding);

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

        private HashSet<string> _immediatePropertyChangeNotificationsToSkipOnce = new HashSet<string>();

        /// <summary>
        /// Will ensure events aren't triggered when value is set
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="value"></param>
        public void SetValue(string propertyPath, object value)
        {
            var property = GetProperty(propertyPath);
            if (property != null)
            {
                string[] paths = propertyPath.Split('.');
                var bindingHost = FindBindingHost(paths);
                if (bindingHost != null)
                {
                    bindingHost._immediatePropertyChangeNotificationsToSkipOnce.Add(paths.Last());
                }

                try
                {
                    property.Item2.SetValue(property.Item1, value);
                }
                finally
                {
                    bindingHost._immediatePropertyChangeNotificationsToSkipOnce.Remove(paths.Last());
                }
            }
        }

        private BindingHost FindBindingHost(string[] paths)
        {
            if (paths.Length == 1)
            {
                return this;
            }

            if (_subPropertyBindings.TryGetValue(paths[0], out BindingHost subBindingHost))
            {
                return subBindingHost.FindBindingHost(paths.Skip(1).ToArray());
            }
            else
            {
                return null;
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