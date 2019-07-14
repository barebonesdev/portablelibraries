using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public class SaveableBase
    {
        protected object _lockForSaving = new object();

        [OnDeserialized]
        public virtual void _onDeserialized()
        {
            _lockForSaving = new object();
        }

        public void Save(Stream stream)
        {

        }
    }
}
