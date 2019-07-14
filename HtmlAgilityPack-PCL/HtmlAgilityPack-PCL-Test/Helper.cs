using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlAgilityPack_PCL_Test
{
    public class Helper
    {
        public static HtmlDocument OpenDocument(string fileName)
        {
            fileName = "HtmlAgilityPack_PCL_Test.Data." + fileName;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(stream);
                return doc;
            }
        }
    }
}
