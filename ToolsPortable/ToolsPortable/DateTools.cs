using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class DateTools
    {
        /// <summary>
        /// See other Last method description. This simply passes in DateTime.Today for "from".
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime Last(DayOfWeek day)
        {
            return Last(day, DateTime.Today);
        }
        /// <summary>
        /// Returns the date that the last specified "day" occurred on, relative to the "from" time. If "day" is Monday and "from" has a DayOfWeek of "Monday", the "from" date will be returned, meaning that it is inclusive of today.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime Last(DayOfWeek day, DateTime from)
        {
            int subtract;

            int currDay;
            if (from.DayOfWeek >= day)
                currDay = (int)from.DayOfWeek;
            else
                currDay = (int)from.DayOfWeek + 7;

            /// Day = Sunday = 0
            /// From = Monday = 1
            /// Subtract = 0 - 1 = -1
            /// 
            /// Day = Wednesday = 3
            /// From = Saturday = 6
            /// Subtract = 3 - 6 = -3

            subtract = (int)day - currDay;


            /// Day = Saturday = 6
            /// From = Sunday = 0
            /// Subtract = 6 - (0 + 7) = -1
            /// 
            /// Day = Wednesday = 3
            /// From = Monday = 1
            /// Subtract = 3 - (1 + 7) = 5
            /// 

            return from.AddDays(subtract).Date;
        }

        /// <summary>
        /// Returns the next date that the specified "day" occurs on, relative to the "from" time. "from" is inclusive.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static DateTime Next(DayOfWeek day, DateTime from)
        {
            int add;

            // If we have to jump the week span to the next week
            if (from.DayOfWeek > day)
            {
                // Find out how many days it takes to get to the next week
                add = 7 - (int)from.DayOfWeek;

                // And then how many days from Sunday to the desired day
                add += (int)day;
            }

            else
            {
                add = (int)day - (int)from.DayOfWeek;
            }

            return from.AddDays(add).Date;
        }

        public static DateTime LastDayOfMonth(DateTime date)
        {
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)), date.Kind);
        }

        public static DateTime GetMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static bool SameMonth(DateTime one, DateTime two)
        {
            if (one.Year == two.Year && one.Month == two.Month)
                return true;

            return false;
        }

        public static bool WithinMonths(DateTime one, DateTime two, int months)
        {
            return Math.Abs(DifferenceInMonths(one, two)) + 1 <= Math.Abs(months);
        }

        /// <summary>
        /// (date - from).Months
        /// </summary>
        /// <param name="date"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static int DifferenceInMonths(DateTime date, DateTime from)
        {
            if (SameMonth(date, from))
            {
                return 0;
            }

            int answer = (date.Year - from.Year) * 12 + date.Month - from.Month;

            if (answer > 0 && from.AddMonths(answer) > date)
            {
                answer--;
            }
            else if (answer < 0 && from.AddMonths(answer) < date)
            {
                answer++;
            }

            return answer;
        }

        private static bool? _is24HourTime;
        public static bool Is24HourTime
        {
            get
            {
                if (_is24HourTime == null)
                    _is24HourTime = new DateTime(2000, 1, 1, 15, 0, 0).ToString().Contains("15");

                return _is24HourTime.Value;
            }
        }

        public static string ToLocalizedString(DayOfWeek day)
        {
            return Last(day).ToString("dddd");
        }

        public static int DaysAhead(DayOfWeek dayInFuture, DayOfWeek dayNow)
        {
            int intDayInFuture = (int)dayInFuture;
            int intDayNow = (int)dayNow;

            if (intDayInFuture >= intDayNow)
            {
                return intDayInFuture - intDayNow;
            }

            intDayInFuture += 7;

            return intDayInFuture - intDayNow;
        }
    }
}
