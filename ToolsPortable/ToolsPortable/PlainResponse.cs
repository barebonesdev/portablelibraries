using System.Runtime.Serialization;

namespace ToolsPortable
{
    [DataContract]
    public class PlainResponse
    {
        [DataMember]
        public string Error;
    }
}
