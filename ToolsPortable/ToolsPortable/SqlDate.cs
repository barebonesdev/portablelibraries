using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class SqlDate
    {
        /// <summary>
        /// Specified as UTC
        /// </summary>
        public static readonly DateTime MinValue = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Specified as UTC
        /// </summary>
        public static readonly DateTime MaxValue = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        public static bool IsValid(DateTime date)
        {
            return date >= MinValue && date <= MaxValue;
        }
    }
}
