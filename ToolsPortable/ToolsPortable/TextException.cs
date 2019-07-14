using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class TextException : Exception
    {
        public TextException(string message) : base(message)
        {
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
