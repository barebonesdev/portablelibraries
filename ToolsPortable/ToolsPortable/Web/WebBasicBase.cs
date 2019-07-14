using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace ToolsPortable.Web
{
    public abstract class WebBasicBase<T> : BareTaskBase<T>
    {
        public static event EventHandler AnyStartedLoading, AnyDoneLoading;

        private void triggerLoading()
        {
            if (UseLoadingEvents && AnyStartedLoading != null)
                AnyStartedLoading(null, new EventArgs());
        }

        private void triggerDone()
        {
            if (UseLoadingEvents && AnyDoneLoading != null)
            {
                if (IsBackground())
                    BeginInvoke(delegate { AnyDoneLoading(null, new EventArgs()); });

                else
                    AnyDoneLoading(null, new EventArgs());
            }
        }

        public string URL { get; private set; }

        /// <summary>
        /// By default this is set to true.
        /// </summary>
        public bool UseLoadingEvents = true;

        public WebBasicBase(string url)
        {
            URL = url;
        }

        private void send()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                hadError(WebErrors.OFFLINE);
                return;
            }

            startRequest();
            triggerLoading();
        }

        protected abstract void startRequest();
        protected abstract void hadError(Exception error);

        protected override void Completed(T result)
        {
            triggerDone();

            base.Completed(result);
        }

        public override void Cancel()
        {
            triggerDone();

            base.Cancel();
        }

        protected override void Start()
        {
            if (IsBackground())
                BeginInvoke(send);
            else
                send();
        }
    }
}
