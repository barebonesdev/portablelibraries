using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsPortable
{
    public class PortableLocalizedResources
    {
        public static Func<string, string> LocalizerExtension;
        public static Func<CultureInfo> CultureExtension;

        public static string GetString(string id)
        {
            if (LocalizerExtension != null)
                return LocalizerExtension.Invoke(id);

            switch (id)
            {
                case "String_TimeToTime":
                    return "{0} to {1}";
            }

            return id;
        }

        public static CultureInfo GetCurrentCulture()
        {
            if (CultureExtension != null)
                return CultureExtension();

            return new CultureInfo("en-US");
        }
    }
}
