using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ToolsPortable.Web
{
    public abstract class WebTimeOffsetBase : BareTaskBase<WebResponse<Stream, int>>
    {
        protected WebTimeOffsetBase() { }

        private static object _lock = new object();
        private static int _offset = int.MinValue;

        private static WebBase<Stream, string> _getTime;

        protected override void Start()
        {
            lock (_lock)
            {
                //wait for previous getTime if it's running
                while (_getTime != null && !_getTime.IsDone)
                    new System.Threading.ManualResetEvent(false).WaitOne(150);

                if (_offset != int.MinValue)
                    Completed(new WebResponse<Stream, int>(null, _offset, null));

                else
                {
                    _getTime = initializeWeb("http://powerplanner.cloudapp.net/api/gettime");
                    //_getTime = initializeWeb("http://localhost:55458/api/gettime");
                    _getTime.Await += _getTime_Await;
                }
            }
        }

        void _getTime_Await(object sender, WebResponse<Stream, string> result)
        {
            lock (_lock)
            {
                if (IsCanceled)
                    return;

                if (result.Error != null)
                    Completed(new WebResponse<Stream, int>(null, 0, result.Error));

                else
                {
                    //comes in like "2013-12-30T02:23:18.1047001Z" with those quotes
                    DateTime time = DateTime.Parse(result.Response.Trim('"')).ToUniversalTime();

                    _offset = (int)((time - _getTime.StartTime).TotalMilliseconds - _getTime.Duration.TotalMilliseconds / 1.2); //we know pinging the server will take longer than response, so we set it to 1.2

                    Completed(new WebResponse<Stream, int>(null, _offset, null));
                }
            }
        }


        public override void Cancel()
        {
            if (_getTime != null)
                _getTime.Cancel();

            base.Cancel();
        }


        protected abstract WebBase<Stream, string> initializeWeb(string url);
    }
}
