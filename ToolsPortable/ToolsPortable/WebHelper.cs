using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class WebHelper
    {
        private static HttpClient _client = new HttpClient();

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
        public static async Task<T> Download<K, T>(string url, K postData, ApiKeyCombo apiKey, Func<K, string> serializeRequest, Func<string, T> deserializeResponse)
        {
            return await Download(url, postData, apiKey, serializeRequest, deserializeResponse, CancellationToken.None);
        }

        public static async Task<T> Download<K, T>(string url, K postData, ApiKeyCombo apiKey, Func<K, string> serializeRequest, Func<string, T> deserializeResponse, CancellationToken cancellationToken)
        {
            return await new WebHelper().DownloadWithCancel(url, postData, apiKey, serializeRequest, deserializeResponse, cancellationToken);
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
        public async Task<T> DownloadWithCancel<K, T>(string url, K postData, ApiKeyCombo apiKey, Func<K, string> serializeRequest, Func<string, T> deserializeResponse)
        {
            return await DownloadWithCancel(url, postData, apiKey, serializeRequest, deserializeResponse, CancellationToken.None);
        }

        public async Task<T> DownloadWithCancel<K, T>(string url, K postData, ApiKeyCombo apiKey, Func<K, string> serializeRequest, Func<string, T> deserializeResponse, CancellationToken cancellationToken)
        {
            if (IsCancelled)
                return default(T);
            
            using (HttpRequestMessage request = new HttpRequestMessage(
                method: postData != null ? HttpMethod.Post : HttpMethod.Get,
                requestUri: new Uri(url)))
            {
                if (postData != null)
                {
                    request.Content = GeneratePostData(request, postData, apiKey, serializeRequest);
                    
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


                    return readResponse(responseStream, deserializeResponse);
                }
            }
        }

        private static T readResponse<T>(Stream response, Func<string, T> deserializeResponse)
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
                            return deserializeResponse(text);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidDataException($"{exception.Message} Response text: {TrimString(text, 200)}", exception);
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

        private static StreamContent GeneratePostData<K>(HttpRequestMessage request, K postData, ApiKeyCombo apiKey, Func<K, string> serializeRequest)
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
                    serialize(postStream, postData, serializeRequest);
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

        private static void serialize<K>(Stream stream, K data, Func<K, string> serializeRequest)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(serializeRequest(data));
            writer.Flush();

#if DEBUG
            stream.Position = 0;
            Debug.WriteLine(new StreamReader(stream).ReadToEnd());
#endif
        }
    }
}
