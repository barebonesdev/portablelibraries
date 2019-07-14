using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ToolsPortable.Web
{
    public class WebResponse<T, K> : EventArgs
    {
        private Exception _error;

        /// <summary>
        /// Returns exception from web call, or if it was a plain response, returns the error from the response
        /// </summary>
        public Exception Error
        {
            get
            {
                if (_error != null)
                    return _error;

                else if (Response is PlainResponse && (Response as PlainResponse).Error != null)
                    return new TextException((Response as PlainResponse).Error);

                return null;
            }
        }

        /// <summary>
        /// If you're getting a Stream as a response and want to use the stream after the Completed event handler finishes, set this to be false. Default is true.
        /// </summary>
        public bool AutoCloseResponseStream = true;

        public T PostItem { get; private set; }
        public K Response { get; private set; }

        public WebResponse(T postItem, K postResponse, Exception error)
        {
            PostItem = postItem;
            Response = postResponse;
            _error = error;
        }
    }
}
