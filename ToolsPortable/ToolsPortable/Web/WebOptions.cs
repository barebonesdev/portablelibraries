using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable.Web
{
    public class WebOptions
    {
        /// <summary>
        /// Already initialized
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public string ContentType;
        public string Method;
    }
}
