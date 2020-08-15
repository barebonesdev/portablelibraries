using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    [DataContract]
    public class BindableBaseWithPortableDispatcher : INotifyPropertyChanged
    {
        private class EventHandlersForDispatcher : IEnumerable<PropertyChangedEventHandler>
        {
            public PortableDispatcher Dispatcher { get; private set; }

            private List<PropertyChangedEventHandler> _eventHandlers = new List<PropertyChangedEventHandler>();

            public EventHandlersForDispatcher(PortableDispatcher dispatcher)
            {
                Dispatcher = dispatcher;
            }

            public void Add(PropertyChangedEventHandler eventHandler)
            {
                if (_eventHandlers.Contains(eventHandler))
                    return;

                _eventHandlers.Add(eventHandler);
            }

            public bool Remove(PropertyChangedEventHandler eventHandler)
            {
                return _eventHandlers.Remove(eventHandler);
            }

            public IEnumerator<PropertyChangedEventHandler> GetEnumerator()
            {
                return _eventHandlers.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Run(Action action)
            {
                if (Dispatcher != null)
                    Dispatcher.Run(action);
                else
                    action.Invoke();
            }
        }

        private class EventHandlersCollection : IEnumerable<EventHandlersForDispatcher>
        {
            private List<EventHandlersForDispatcher> _list = new List<EventHandlersForDispatcher>();

            public void Add(PortableDispatcher dispatcher, PropertyChangedEventHandler eventHandler)
            {
                EventHandlersForDispatcher handlersForDispatcher = _list.FirstOrDefault(i => i.Dispatcher == dispatcher);

                if (handlersForDispatcher == null)
                {
                    handlersForDispatcher = new EventHandlersForDispatcher(dispatcher);
                    _list.Add(handlersForDispatcher);
                }

                // If the event handler isn't in the list yet, add it
                if (!handlersForDispatcher.Contains(eventHandler))
                    handlersForDispatcher.Add(eventHandler);
            }

            public void Remove(PropertyChangedEventHandler eventHandler)
            {
                List<EventHandlersForDispatcher> toRemove = new List<EventHandlersForDispatcher>();

                foreach (var dispatcherHandlers in _list)
                {
                    // If we removed an event handler, and it's the last one in that list, we need to remove that key too
                    if (dispatcherHandlers.Remove(eventHandler) && !dispatcherHandlers.Any())
                        toRemove.Add(dispatcherHandlers);
                }

                // Remove the keys that we flagged
                foreach (var d in toRemove)
                    _list.Remove(d);
            }

            public IEnumerator<EventHandlersForDispatcher> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private EventHandlersCollection _eventHandlersCollection;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    if (_eventHandlersCollection == null)
                        _eventHandlersCollection = new EventHandlersCollection();

                    PortableDispatcher d = PortableDispatcher.GetCurrentDispatcher();

                    _eventHandlersCollection.Add(d, value);
                }
            }

            remove
            {
                lock (this)
                {
                    if (_eventHandlersCollection == null)
                        return;

                    _eventHandlersCollection.Remove(value);
                }
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyNames">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            object sender = this;

            try
            {
                lock (this)
                {
                    if (_eventHandlersCollection != null)
                    {
                        foreach (var pair in _eventHandlersCollection)
                        {
                            // Create a new object since the handlers might change by the time dispatched to UI
                            PropertyChangedEventHandler[] eventHandlers = pair.ToArray();

                            // Don't need to wait for these, UI's can update in parallel
                            pair.Run(delegate
                            {
                                foreach (var eventHandler in eventHandlers)
                                    foreach (string propertyName in propertyNames)
                                        eventHandler(sender, new PropertyChangedEventArgs(propertyName));
                            });
                        }
                    }
                }
            }

            catch { }
        }


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
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperties<T>(ref T storage, T value, params string[] propertyNames)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyNames);
            return true;
        }

        public static Func<PortableDispatcher> ObtainDispatcherFunction { get; set; }

        private object m_cachedComputationPropertiesLock;
        private Dictionary<string, CachedComputationProperty> m_cachedComputationProperties;
        protected T CachedComputation<T>(Func<T> computation, string[] dependentOn, [CallerMemberName]string propertyName = null)
        {
            // Have to initialize this here since this method gets called before any class initializers are performed (not sure why exactly, probably because it's used in property values in parent class)
            // But there shouldn't be any cross-threading concerns since that's part of the overall initialization anyways too
            if (m_cachedComputationPropertiesLock == null)
            {
                m_cachedComputationPropertiesLock = new object();
            }


            if (propertyName == null)
            {
                // This should theoretically never be null, but seems like Android, when running in the widget at least, it ends up being null?
                // Adding this check to confirm.
                throw new ArgumentNullException(nameof(propertyName));
            }

            CachedComputationProperty prop;

            lock (m_cachedComputationPropertiesLock)
            {
                if (m_cachedComputationProperties == null)
                {
                    m_cachedComputationProperties = new Dictionary<string, CachedComputationProperty>();
                }

                if (!m_cachedComputationProperties.TryGetValue(propertyName, out prop))
                {
                    prop = new CachedComputationProperty(this, propertyName, delegate { return computation(); });
                    m_cachedComputationProperties[propertyName] = prop;

                    ListenToProperties(dependentOn, prop.NotifyDependentOnValueChanged);
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
            private BindableBaseWithPortableDispatcher m_parent;
            private Func<object> m_computation;
            private string m_propertyName;
            private bool m_hasBeenRequested;

            public CachedComputationProperty(BindableBaseWithPortableDispatcher parent, string thisPropertyName, Func<object> computation)
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
                    var newVal = GetValue();
                    if (!object.Equals(currVal, newVal))
                    {
                        // Change this back to false, so that we can see if anyone actually asks for the new value again,
                        // since maybe they've stopped listening since then, and we can stop sending notifications
                        m_hasBeenRequested = false;

                        // Notify it was changed
                        m_parent.OnPropertyChanged(m_propertyName);
                    }
                }
            }
        }

        private Dictionary<string, List<Action>> m_propertyChangedActions;
        /// <summary>
        /// Perform an action if the property was changed
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <param name="action"></param>
        protected void ListenToProperties(string[] propertyNames, Action action)
        {
            if (m_propertyChangedActions == null)
            {
                m_propertyChangedActions = new Dictionary<string, List<Action>>();

                this.PropertyChanged += OwnPropertyChanged;
            }

            foreach (var propertyName in propertyNames)
            {
                List<Action> actions;
                if (!m_propertyChangedActions.TryGetValue(propertyName, out actions))
                {
                    actions = new List<Action>();
                    m_propertyChangedActions[propertyName] = actions;
                }

                actions.Add(action);
            }
        }

        private void OwnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Action[] actions = null;
            lock (m_cachedComputationPropertiesLock)
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
    }
}
