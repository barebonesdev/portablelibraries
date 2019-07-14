using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolsPortable.Resources;

namespace ToolsPortable
{
    public class DateStrings
    {
        /// <summary>
        /// Returns something like "IN THE PAST", "TODAY", "IN TWO DAYS", "NEXT WEDNESDAY", etc
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToFriendly(DateTime date)
        {
            if (date < DateTime.Today)
                return ToolsPortableResources.DateStrings_Friendly_InThePast;
            else if (date == DateTime.Today)
                return ToolsPortableResources.DateStrings_Friendly_Today;
            else if (date == DateTime.Today.AddDays(1))
                return ToolsPortableResources.DateStrings_Friendly_Tomorrow;
            else if (date == DateTime.Today.AddDays(2))
                return ToolsPortableResources.DateStrings_Friendly_InTwoDays;
            else if (date < DateTime.Today.AddDays(7))
                return ToolsPortableResources.DateStrings_Friendly_This + " " + date.ToString("dddd").ToUpper();
            else if (date < DateTime.Today.AddDays(14))
                return ToolsPortableResources.DateStrings_Friendly_Next + " " + date.ToString("dddd").ToUpper();
            else
                return date.ToString("dddd, MMMM d").ToUpper();
        }

        /// <summary>
        /// Returns something like "over a year ago", "a few days ago", "today, 5:30 pm", "in two days, 4:20 pm", etc
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToFriendlyDateAndTime(DateTime date)
        {
            if (date < DateTime.Today.AddYears(-1))
                return "over a year ago";

            if (date < DateTime.Today.AddMonths(-4))
                return "a year ago";

            if (date < DateTime.Today.AddMonths(-1))
                return "a few months ago";

            if (date < DateTime.Today.AddDays(-4))
                return "a month ago";

            if (date < DateTime.Today.AddDays(-1))
                return "a few days ago";

            if (date < DateTime.Today)
                return "yesterday";

            if (date < DateTime.Today.AddDays(1))
                return "today, " + date.ToString("t").ToLower();

            if (date < DateTime.Today.AddDays(2))
                return "tomorrow, " + date.ToString("t").ToLower();

            if (date < DateTime.Today.AddDays(3))
                return "in two days, " + date.ToString("t").ToLower();

            if (date < DateTime.Today.AddDays(7))
                return "this week";

            if (date < DateTime.Today.AddDays(14))
                return "next week";

            if (date.Year == DateTime.Today.Year)
                return date.ToString("MMMM").ToLower();

            return date.ToString("y").ToLower();
        }
    }
}
