using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public class ImageRequest
    {
        [DataMember]
        public string ImageName;
    }

    [DataContract]
    public class ImageResponse : PlainResponse
    {
        [DataMember]
        public byte[] ImageData;
    }
}
