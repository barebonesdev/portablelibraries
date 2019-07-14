using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HtmlAgilityPack
{
    [DataContract]
    public class HtmlInstruction
    {
        public HtmlInstruction(string xpath, string selectAttribute = null)
        {
            XPath = xpath;
            SelectAttribute = selectAttribute;
        }

        [DataMember]
        public string XPath;

        /// <summary>
        /// If this is null, it should simply select the inner text. Otherwise, it should select the attribute.
        /// </summary>
        [DataMember]
        public string SelectAttribute;
    }
}
