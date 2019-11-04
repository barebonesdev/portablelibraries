using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class CalendarArray
    {
        public static DateTime[,] Generate(DateTime month, DayOfWeek firstDayOfWeek)
        {
            month = new DateTime(month.Year, month.Month, 1);

            DateTime[,] array = new DateTime[6, 7];

            int daysOfLastMonth = DateTools.DaysAhead(month.DayOfWeek, firstDayOfWeek) - 1;
            if (month.DayOfWeek == firstDayOfWeek)
                daysOfLastMonth = 6;

            for (DateTime lastMonth = month.AddDays(-1); daysOfLastMonth >= 0; daysOfLastMonth--, lastMonth = lastMonth.AddDays(-1))
                array[0, daysOfLastMonth] = lastMonth;

            //finish first row
            int firstRow = DateTools.DaysAhead(month.DayOfWeek, firstDayOfWeek);
            if (month.DayOfWeek == firstDayOfWeek)
                firstRow = 0;

            DateTime currMonth = month;

            for (; firstRow < 7; firstRow++, currMonth = currMonth.AddDays(1))
                array[0, firstRow] = currMonth;

            for (int row = 1; row < 6; row++)
                for (int col = 0; col < 7; col++, currMonth = currMonth.AddDays(1))
                    array[row, col] = currMonth;

            return array;
        }
    }
}
