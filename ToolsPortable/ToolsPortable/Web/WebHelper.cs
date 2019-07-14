using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ToolsPortable.Web
{
    public abstract class WebHelper<T> : WebBasicBase<T>
    {
        public HttpWebRequest Request { get; private set; }
        public HttpWebResponse Response { get; private set; }
        protected abstract bool HasPostStream { get; }

        /// <summary>
        /// The time that the actual request was sent, in UTC
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// The time that the response was received, in UTC
        /// </summary>
        public DateTime EndTime { get; private set; }
        public TimeSpan Duration
        {
            get { return EndTime - StartTime; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options">If null, it'll use the default web options (constructs a new WebObtions item)</param>
        public WebHelper(string url)
            : base(url)
        {
            Request = (HttpWebRequest)WebRequest.Create(URL);
        }

        protected override void startRequest()
        {
            if (HasPostStream)
                Request.BeginGetRequestStream(gotRequest, Request);

            else
                startResponse();
        }

        private void startResponse()
        {
            StartTime = DateTime.UtcNow;
            Request.BeginGetResponse(gotResponse, Request);
        }

        private void gotRequest(IAsyncResult result)
        {
            if (IsCanceled)
                return;

            try
            {
                using (Stream stream = Request.EndGetRequestStream(result))
                {
                    writeRequest(stream);
                }

                if (IsCanceled)
                    return;

                startResponse();
            }

            catch (Exception ex) { hadError(ex); }
        }

        public override void Cancel()
        {
            if (Request != null)
            {
                try { Request.Abort(); }
                catch { }
            }

            base.Cancel();
        }

        private void gotResponse(IAsyncResult result)
        {
            EndTime = DateTime.UtcNow;

            if (IsCanceled)
                return;

            try
            {
                Response = (HttpWebResponse)Request.EndGetResponse(result);

                readResponse(Response.GetResponseStream());
                //using (Stream answer = response.GetResponseStream())
                //{
                //    readResponse(answer);
                //}
            }

            catch (Exception ex) { hadError(ex); }
        }

        /// <summary>
        /// sets a header on the request. Can be called anytime before the actual Response is started.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        //protected void setHeader(string name, string value)
        //{
        //    if (Request == null)
        //        Options.Headers[name] = value;

        //    else
        //        Request.Headers[name] = value;
        //}

        /// <summary>
        /// Will be try/caught. Must call Completed unless it throws exception. Response stream will be 
        /// </summary>
        /// <param name="response"></param>
        protected abstract void readResponse(Stream response);

        /// <summary>
        /// Is triggered on background thread, will only be called if HasPostStream is true
        /// </summary>
        /// <param name="requestStream"></param>
        protected abstract void writeRequest(Stream requestStream);
    }
}
