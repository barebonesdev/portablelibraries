using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using ToolsPortable.Web;

namespace ToolsPortable
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K">If using Stream, it's your responsibility to close the stream.</typeparam>
    public abstract class WebBase<T, K> : WebHelper<WebResponse<T, K>>
    {
        public enum Serializer
        {
            DataContractJson,
            JsonNET
        }

        public T PostData;
        public ApiKeyCombo ApiKey;
        public IEnumerable<Type> KnownTypes;
        public Serializer PostSerializer = Serializer.DataContractJson;

        /// <summary>
        /// Automatically sets ContentType to application/json if posting something that's not a stream, and auto sets Method based on PostData
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">Can be null</param>
        /// <param name="options"></param>
        public WebBase(string url)
            : base(url)
        {
        }

        protected override void Start()
        {
            if (ApiKey != null)
                base.Request.Headers["HashedKey"] = ApiKey.HashedKey;

            //if we'll be serializing data, set content type
            if (PostData != null && !(PostData is Stream) && !(PostData is string) && base.Request.ContentType == null)
                base.Request.ContentType = "application/json";

            //if we'll be deserializing data, set accept type
            if (typeof(K) != typeof(Stream) && typeof(K) != typeof(string) && base.Request.Accept == null)
                base.Request.Accept = "application/json";


            //automatically defaults to GET
            if (PostData != null)
                base.Request.Method = "POST";

            base.Start();
        }

        protected override bool HasPostStream
        {
            get { return PostData != null; }
        }

        protected override void readResponse(System.IO.Stream response)
        {
            if (typeof(K) == typeof(Stream))
            {
                //MemoryStream copy = new MemoryStream((int)response.Length);
                //response.CopyTo(copy);
                //Completed(new WebResponse<T, K>(PostData, (K)(object)copy, null));

                //will be auto-closed after completed finishes unless user requested otherwise
                Completed(new WebResponse<T, K>(PostData, (K)(object)response, null));
            }

            else if (typeof(K) == typeof(string))
            {
                string answer = null;

                try
                {
                    using (StreamReader reader = new StreamReader(response))
                    {
                        answer = reader.ReadToEnd();
                    }
                }

                finally { response.Dispose(); }

                Completed(new WebResponse<T, K>(PostData, (K)(object)answer, null));
            }

            else
            {
                K answer;
                try
                {
                    answer = (K)new DataContractJsonSerializer(typeof(K), KnownTypes).ReadObject(response);
                }

#if DEBUG
                catch (Exception e)
                {
                    response.Position = 0;
                    Debug.WriteLine("WebBase Deserialization Error:\n\n" + new StreamReader(response).ReadToEnd());
                    throw e;
                }
#endif

                finally { response.Dispose(); }

                Completed(new WebResponse<T, K>(PostData, answer, null));
            }
        }

        protected override void AfterCompleted(WebResponse<T, K> result)
        {
            if (result.AutoCloseResponseStream && result.Response is Stream)
            {
                (result.Response as Stream).Dispose();
            }
        }

        protected override void writeRequest(Stream requestStream)
        {
            if (ApiKey != null)
            {
                Stream s;

                if (PostData is Stream)
                    s = PostData as Stream;

                else
                {
                    s = new MemoryStream();

                    switch (PostSerializer)
                    { 
                        case Serializer.DataContractJson:
                            new DataContractJsonSerializer(typeof(T), KnownTypes).WriteObject(s, PostData);
                            break;

                        case Serializer.JsonNET:
                            StreamWriter writer = new StreamWriter(s);
                            new JsonSerializer().Serialize(writer, PostData);
                            writer.Flush();
                            break;
                    }

                    s.Position = 0;
                }

                //turn it into bytes
                byte[] bytes = new byte[s.Length - s.Position];
                s.Read(bytes, 0, bytes.Length);

                //hash the bytes, then hash ApiKey + Bytes
                string hashedData = sha1(bytes);
                hashedData = sha256(ApiKey.ApiKey + hashedData);

                //set the header with the hashed data
                base.Request.Headers["HashedData"] = hashedData;

                //write the data into the request stream
                requestStream.Write(bytes, 0, bytes.Length);
            }

            else
            {
                if (PostData is Stream)
                {
                    Stream s = PostData as Stream;

                    //turn it into bytes
                    byte[] bytes = new byte[s.Length - s.Position];
                    s.Read(bytes, 0, bytes.Length);

                    //write the data into the request stream
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                else
                    new DataContractJsonSerializer(typeof(T)).WriteObject(requestStream, PostData);
            }
        }

        protected abstract string sha256(string str);
        protected abstract string sha1(byte[] bytes);

        protected override void hadError(Exception error)
        {
            Completed(new WebResponse<T, K>(PostData, default(K), error));
        }
    }
}
