using System;
using System.Collections.Generic;
using System.Text;

namespace BareMvvm.Core.Binding
{
    public class BindingRegistration
    {
        private BindingHost _host;
        internal string PropertyName { get; private set; }
        internal BindingHost.InternalBindingRegistration InternalRegistration { get; private set; }
        internal BindingRegistration SubRegistration { get; private set; }

        internal BindingRegistration(BindingHost host, string propertyName, BindingHost.InternalBindingRegistration internalRegistration)
        {
            _host = host;
            PropertyName = propertyName;
            InternalRegistration = internalRegistration;
        }

        internal BindingRegistration(BindingHost host, string propertyName, BindingRegistration subRegistration)
        {
            _host = host;
            PropertyName = propertyName;
            SubRegistration = subRegistration;
        }

        public void Unregister()
        {
            _host.UnregisterBinding(this);
        }
    }
}
