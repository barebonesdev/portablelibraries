using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ToolsPortable.Web
{
    public static class WebErrors
    {
        public static readonly WebException OFFLINE = new WebException("You are not connected to the internet.", WebExceptionStatus.UnknownError);

        public static bool IsOffline(WebException ex)
        {
            return ex != null && ex.Status == WebExceptionStatus.UnknownError;
        }
    }
}
