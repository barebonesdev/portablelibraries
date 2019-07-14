using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class ApiKeyCombo
    {
        public string ApiKey { get; private set; }
        public string HashedKey { get; private set; }

        public ApiKeyCombo(string apiKey, string hashedKey)
        {
            if (apiKey == null || hashedKey == null)
                throw new ArgumentNullException("Both ApiKey and HashedKey must be initialized values!");

            ApiKey = apiKey;
            HashedKey = hashedKey;
        }
    }
}
