using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    [DataContract]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        private void BindableBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, params string[] propertyNames)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyNames);
            return true;
        }

        protected bool SetProperty<T>(ref T storage, T value, string[] propertyNames, params Action[] changedActions)
        {
            if (SetProperty(ref storage, value, propertyNames))
            {
                foreach (var a in changedActions)
                {
                    a();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                    eventHandler(this, new PropertyChangedEventArgs(propertyNames[i]));
            }
        }

        private Dictionary<string, object> _values;
        private Dictionary<string, object> values
        {
            get
            {
                if (_values == null)
                    _values = new Dictionary<string, object>();

                return _values;
            }
        }

        protected bool SetValue<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(GetValue<T>(propertyName), value)) return false;

            values[propertyName] = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetValue<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected T GetValue<T>([CallerMemberName]string propertyName = null)
        {
            object obj;

            if (values.TryGetValue(propertyName, out obj))
                return (T)obj;

            return default(T);
        }

        protected T GetValueOrDefault<T>(T defaultValue, [CallerMemberName]string propertyName = null)
        {
            object obj;

            if (values.TryGetValue(propertyName, out obj))
                return (T)obj;

            return defaultValue;
        }

        private Dictionary<string, List<Action>> m_propertyChangedActions;
        /// <summary>
        /// Perform an action if the property was changed
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="action"></param>
        protected void ListenToProperty(string propertyName, Action action)
        {
            if (m_propertyChangedActions == null)
            {
                m_propertyChangedActions = new Dictionary<string, List<Action>>();

                this.PropertyChanged += OwnPropertyChanged;
            }

            List<Action> actions;
            if (!m_propertyChangedActions.TryGetValue(propertyName, out actions))
            {
                actions = new List<Action>();
                m_propertyChangedActions[propertyName] = actions;
            }

            actions.Add(action);
        }

        private void OwnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Action[] actions = null;

            lock (m_cachedComputationLock)
            {
                if (m_propertyChangedActions.TryGetValue(e.PropertyName, out List<Action> actionsList))
                {
                    actions = actionsList.ToArray();
                }
            }

            if (actions != null)
            {
                foreach (var a in actions)
                {
                    a();
                }
            }
        }

        private object m_cachedComputationLock;
        private Dictionary<string, CachedComputationProperty> m_cachedComputationProperties;
        protected T CachedComputation<T>(Func<T> computation, string[] dependentOn, [CallerMemberName]string propertyName = null)
        {
            // Have to initialize this here since this method gets called before any class initializers are performed (not sure why exactly, probably because it's used in property values in parent class)
            // But there shouldn't be any cross-threading concerns since that's part of the overall initialization anyways too
            if (m_cachedComputationLock == null)
            {
                m_cachedComputationLock = new object();
            }

            CachedComputationProperty prop;

            lock (m_cachedComputationLock)
            {
                if (m_cachedComputationProperties == null)
                {
                    m_cachedComputationProperties = new Dictionary<string, CachedComputationProperty>();
                }

                if (!m_cachedComputationProperties.TryGetValue(propertyName, out prop))
                {
                    prop = new CachedComputationProperty(this, propertyName, delegate { return computation(); });
                    m_cachedComputationProperties[propertyName] = prop;

                    foreach (var dependentOnProperty in dependentOn)
                    {
                        ListenToProperty(dependentOnProperty, prop.NotifyDependentOnValueChanged);
                    }
                }
            }

            object val = prop.GetValue();
            if (val == null)
            {
                return default(T);
            }
            return (T)val;
        }

        private class CachedComputationProperty
        {
            private bool m_isUpToDate;
            private object m_currentValue;
            private BindableBase m_parent;
            private Func<object> m_computation;
            private string m_propertyName;
            private bool m_hasBeenRequested;

            public CachedComputationProperty(BindableBase parent, string thisPropertyName, Func<object> computation)
            {
                m_parent = parent;
                m_computation = computation;
                m_propertyName = thisPropertyName;
            }

            public object GetValue()
            {
                m_hasBeenRequested = true;

                if (!m_isUpToDate)
                {
                    m_currentValue = m_computation();
                    m_isUpToDate = true;
                }

                return m_currentValue;
            }

            public void NotifyDependentOnValueChanged()
            {
                // Only if it's up-to-date and has been requested should we compute
                if (m_isUpToDate && m_hasBeenRequested)
                {
                    m_isUpToDate = false;
                    var currVal = m_currentValue;
                    var newVal = GetValue(); // This will set the value and uptodate=true
                    if (!object.Equals(currVal, newVal))
                    {
                        // Change this back to false, so that we can see if anyone actually asks for the new value again,
                        // since maybe they've stopped listening since then, and we can stop sending notifications
                        m_hasBeenRequested = false;

                        // Notify it was changed
                        m_parent.OnPropertyChanged(m_propertyName);
                    }
                }
                else
                {
                    m_isUpToDate = false;
                }
            }
        }
    }
}
