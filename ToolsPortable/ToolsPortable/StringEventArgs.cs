using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class StringEventArgs : EventArgs
    {
        public string String { get; private set; }

        public StringEventArgs(string str)
        {
            String = str;
        }
    }
}
