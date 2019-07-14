using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ToolsPortable
{
    [DataContract]
    public class PlainResponse
    {
        [DataMember]
        public string Error;
    }

    [DataContract]
    public class ByteRequest
    {
        public ByteRequest(object data) : this(data, new Type[0]) { }
        public ByteRequest(object data, IEnumerable<Type> knownTypes)
        {
            Stream stream = new MemoryStream();
            new DataContractJsonSerializer(data.GetType(), knownTypes).WriteObject(stream, data);

            stream.Position = 0;
            RawData = new byte[stream.Length];
            stream.Read(RawData, 0, RawData.Length);
        }

        [DataMember]
        public byte[] RawData;

        public T Deserialize<T>()
        {
            return Deserialize<T>(new Type[0]);
        }

        public T Deserialize<T>(IEnumerable<Type> knownTypes)
        {
            Stream stream = new MemoryStream(RawData.Length);
            stream.Write(RawData, 0, RawData.Length);
            stream.Position = 0;
            return (T)new DataContractJsonSerializer(typeof(T), knownTypes).ReadObject(stream);
        }

        public static void Serialize(Stream streamToWriteTo, object data)
        {
            Serialize(streamToWriteTo, data, new Type[0]);
        }

        public static void Serialize(Stream streamToWriteTo, object data, IEnumerable<Type> knownTypes)
        {
            ByteRequest request = new ByteRequest(data, knownTypes);
            new DataContractJsonSerializer(typeof(ByteRequest)).WriteObject(streamToWriteTo, request);
        }

        public static T Deserialize<T>(byte[] RawData) { return Deserialize<T>(RawData, new Type[0]); }
        public static T Deserialize<T>(byte[] RawData, IEnumerable<Type> knownTypes)
        {
            Stream stream = new MemoryStream(RawData.Length);
            stream.Write(RawData, 0, RawData.Length);
            stream.Position = 0;
            return (T)new DataContractJsonSerializer(typeof(T), knownTypes).ReadObject(stream);
        }

        public static byte[] Serialize(object data)
        {
            Stream stream = new MemoryStream();
            new DataContractJsonSerializer(data.GetType()).WriteObject(stream, data);

            stream.Position = 0;
            byte[] answer = new byte[stream.Length];
            stream.Read(answer, 0, answer.Length);
            return answer;
        }
    }
}
