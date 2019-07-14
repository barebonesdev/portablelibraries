using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ToolsPortable
{
    /// <summary>
    /// You must hold a reference to this class while you want the event alive, otherwise it'll be disposed
    /// </summary>
    public class BindablePropertyWatcher
    {
        public INotifyPropertyChanged Source { get; private set; }
        public string PropertyName { get; private set; }
        private Action _propertyChangedAction;
        private PropertyInfo _propertyInfo;

        public BindablePropertyWatcher(INotifyPropertyChanged source, string propertyName, Action propertyChangedAction)
        {
            Source = source;
            PropertyName = propertyName;
            _propertyChangedAction = propertyChangedAction;

            source.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(Source_PropertyChanged).Handler;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(PropertyName))
            {
                _propertyChangedAction();
            }
        }

        /// <summary>
        /// Gets the current value of the property.
        /// </summary>
        /// <returns></returns>
        public TSource GetValue<TSource>()
        {
            if (_propertyInfo == null)
            {
                _propertyInfo = Source.GetType().GetRuntimeProperty(PropertyName);
            }

            return (TSource)_propertyInfo.GetValue(Source);
        }
    }
}
