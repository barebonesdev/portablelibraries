using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    /// <summary>
    /// DEPRECIATED, just use QueryString
    /// </summary>
    public class StringQuery : List<KeyValuePair<string, string>>
    {
        /// <summary>
        /// DEPRECIATED, just use QueryString. Does NOT decode
        /// </summary>
        /// <param name="navigationUri"></param>
        public StringQuery(string navigationUri)
        {
            try
            {
                if (navigationUri == null)
                    return;

                int index = navigationUri.IndexOf('?');
                if (index > 0)
                {
                    string buildingKey = "";
                    string buildingValue = null;

                    for (int i = index + 1; i < navigationUri.Length; i++)
                    {
                        char c = navigationUri[i];

                        if (buildingValue == null)
                        {
                            if (c == '=')
                                buildingValue = "";

                            else
                                buildingKey += c;
                        }

                        else
                        {
                            if (c == '&')
                            {
                                base.Add(new KeyValuePair<string, string>(buildingKey, buildingValue));
                                buildingValue = null;
                                buildingKey = "";
                            }

                            else
                                buildingValue += c;
                        }
                    }

                    if (buildingValue != null)
                        base.Add(new KeyValuePair<string, string>(buildingKey, buildingValue));
                }
            }

            catch { }
        }

        /// <summary>
        /// Returns the value of the first matching key, or return null if it wasn't found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetOrNull(string key)
        {
            for (int i = 0; i < base.Count; i++)
                if (base[i].Key.Equals(key))
                    return base[i].Value;

            return null;
        }

        public bool ContainsKey(string key)
        {
            for (int i = 0; i < base.Count; i++)
                if (base[i].Key.Equals(key))
                    return true;

            return false;
        }

        public string this[string key]
        {
            get
            {
                string answer = GetOrNull(key);

                if (answer == null)
                    throw new ArgumentException("Key not found");

                return answer;
            }
        }
    }
}
