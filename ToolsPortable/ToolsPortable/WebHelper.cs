using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class WebHelper
    {
        private static HttpClient _client = new HttpClient();

        public enum Serializer
        {
            JsonNET,
            DataContractJson
        }

        //private HttpWebRequest _req;

        public bool IsCancelled
        {
            get;
            private set;
        }

        public void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        /// Supports streams and strings.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task<T> Download<K, T>(string url, K postData, ApiKeyCombo apiKey, Serializer serializer = Serializer.DataContractJson)
        {
            return await Download<K, T>(url, postData, apiKey, serializer, CancellationToken.None);
        }

        public static async Task<T> Download<K, T>(string url, K postData, ApiKeyCombo apiKey, CancellationToken cancellationToken)
        {
            return await Download<K, T>(url, postData, apiKey, Serializer.DataContractJson, cancellationToken);
        }

        public static async Task<T> Download<K, T>(string url, K postData, ApiKeyCombo apiKey, Serializer serializer, CancellationToken cancellationToken)
        {
            return await new WebHelper().DownloadWithCancel<K, T>(url, postData, apiKey, serializer, cancellationToken);
        }

        /// <summary>
        /// Supports streams and strings.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<T> DownloadWithCancel<K, T>(string url, K postData, ApiKeyCombo apiKey, Serializer serializer = Serializer.DataContractJson)
        {
            return await DownloadWithCancel<K, T>(url, postData, apiKey, serializer, CancellationToken.None);
        }

        public async Task<T> DownloadWithCancel<K, T>(string url, K postData, ApiKeyCombo apiKey, Serializer serializer, CancellationToken cancellationToken)
        {
            if (IsCancelled)
                return default(T);
            
            using (HttpRequestMessage request = new HttpRequestMessage(
                method: postData != null ? HttpMethod.Post : HttpMethod.Get,
                requestUri: new Uri(url)))
            {
                if (postData != null)
                {
                    request.Content = GeneratePostData(request, postData, apiKey, serializer);
                    
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (apiKey != null)
                    request.Headers.Add("HashedKey", apiKey.HashedKey);

                //if we'll be deserializing data, set accept type
                if (typeof(K) != typeof(Stream) && typeof(K) != typeof(string))
                    request.Headers.Add("Accept", "application/json");

                if (IsCancelled)
                    return default(T);
                
                using (HttpResponseMessage response = await _client.SendAsync(request))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (IsCancelled)
                        return default(T);

                    // If non-200 status code, throws exception
                    response.EnsureSuccessStatusCode();

                    Stream responseStream = await response.Content.ReadAsStreamAsync();


                    cancellationToken.ThrowIfCancellationRequested();

                    if (IsCancelled)
                        return default(T);


                    return readResponse<T>(responseStream);
                }
            }
        }

        private static T readResponse<T>(Stream response)
        {
            if (typeof(T) == typeof(Stream))
            {
                return (T)(object)response;
            }

            else if (typeof(T) == typeof(string))
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

                return (T)(object)answer;
            }

            else
            {
                try
                {
                    //string rawResponse = new StreamReader(response).ReadToEnd();
                    //Debug.WriteLine(rawResponse);

                    using (StreamReader reader = new StreamReader(response))
                    {
                        string text = reader.ReadToEnd();

                        try
                        {
                            return JsonConvert.DeserializeObject<T>(text);
                        }
                        catch (JsonReaderException jsonReaderException)
                        {
                            throw new JsonReaderException($"{jsonReaderException.Message} Response text: {TrimString(text, 200)}", jsonReaderException);
                        }
                    }

                    //answer = (T)new DataContractJsonSerializer(typeof(T)).ReadObject(response);
                }

                //#if DEBUG
                //doesn't work since can't seek backwards on stream
                //catch (Exception e)
                //{
                //    response.Position = 0;
                //    string rawResponse = new StreamReader(response).ReadToEnd();
                //    Debug.WriteLine("WebBase Deserialization Error:\n\n" + rawResponse);
                //    throw e;
                //}
                //#endif

                finally { response.Dispose(); }
            }
        }

        private static string TrimString(string str, int length)
        {
            if (str.Length > length)
            {
                return str.Substring(0, length) + "...";
            }

            return str;
        }

        private static StreamContent GeneratePostData<K>(HttpRequestMessage request, K postData, ApiKeyCombo apiKey, Serializer serializer)
        {
            Stream postStream = new MemoryStream();
            string hashedData = null;

            try
            {
                if (postData is Stream)
                {
                    (postData as Stream).CopyTo(postStream);
                    postStream.Position = 0;
                }

                else
                {
                    serialize(postStream, postData, serializer);
                    postStream.Position = 0;
                }


                if (apiKey != null)
                {
                    // Turn it into bytes
                    byte[] bytes = new byte[postStream.Length];
                    postStream.Read(bytes, 0, bytes.Length);
                    postStream.Position = 0;

                    //hash the bytes, then hash ApiKey + Bytes
                    hashedData = EncryptionHelper.Sha1(bytes);
                    hashedData = EncryptionHelper.Sha256(apiKey.ApiKey + hashedData);
                }
            }

            catch
            {
                postStream.Dispose();
                throw;
            }

            StreamContent content = new StreamContent(postStream);

            if (hashedData != null)
                request.Headers.Add("HashedData", hashedData);

            // If we serialized as JSON
            if (!(postData is Stream))
                content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");

            return content;
        }

        private static void serialize(Stream stream, object data, Serializer serializer)
        {
            switch (serializer)
            {
                case Serializer.DataContractJson:
                    new DataContractJsonSerializer(data.GetType()).WriteObject(stream, data);
                    break;

                case Serializer.JsonNET:

                    StreamWriter writer = new StreamWriter(stream);
                    new JsonSerializer().Serialize(writer, data);
                    writer.Flush();

#if DEBUG
                    stream.Position = 0;
                    Debug.WriteLine(new StreamReader(stream).ReadToEnd());
#endif
                    break;
            }
        }
    }
}
