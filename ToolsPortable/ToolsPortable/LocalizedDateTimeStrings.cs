using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class LocalizedDateTimeStrings
    {
        public class _dayNamesClass
        {
            public static string Sunday
            {
                get { return GetDayName(DayOfWeek.Sunday); }
            }

            public static string Monday
            {
                get { return GetDayName(DayOfWeek.Monday); }
            }

            public static string Tuesday
            {
                get { return GetDayName(DayOfWeek.Tuesday); }
            }

            public static string Wednesday
            {
                get { return GetDayName(DayOfWeek.Wednesday); }
            }

            public static string Thursday
            {
                get { return GetDayName(DayOfWeek.Thursday); }
            }

            public static string Friday
            {
                get { return GetDayName(DayOfWeek.Friday); }
            }

            public static string Saturday
            {
                get { return GetDayName(DayOfWeek.Saturday); }
            }
        }

        public class _abbreviatedDayNamesClass
        {
            public static string Sunday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Sunday); }
            }

            public static string Monday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Monday); }
            }

            public static string Tuesday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Tuesday); }
            }

            public static string Wednesday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Wednesday); }
            }

            public static string Thursday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Thursday); }
            }

            public static string Friday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Friday); }
            }

            public static string Saturday
            {
                get { return GetAbbreviatedDayName(DayOfWeek.Saturday); }
            }
        }


        /// <summary>
        /// Here so that XAML can reference it
        /// </summary>
        public _dayNamesClass DayNames
        {
            get
            {
                return new _dayNamesClass();
            }
        }

        /// <summary>
        /// Here so XAML can reference it
        /// </summary>
        public _abbreviatedDayNamesClass AbbreviatedDayNames
        {
            get { return new _abbreviatedDayNamesClass(); }
        }
        


        public static string GetDayName(DayOfWeek dayOfWeek)
        {
            return DateTimeFormatInfo.CurrentInfo.GetDayName(dayOfWeek);
        }

        public static string GetAbbreviatedDayName(DayOfWeek dayOfWeek)
        {
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(dayOfWeek);
        }
    }
}
