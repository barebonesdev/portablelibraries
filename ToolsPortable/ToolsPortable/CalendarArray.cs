using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public static class CalendarArray
    {
        public static DateTime[,] Generate(DateTime month)
        {
            month = new DateTime(month.Year, month.Month, 1);

            DateTime[,] array = new DateTime[6, 7];

            int daysOfLastMonth = (int)month.DayOfWeek - 1;
            if (month.DayOfWeek == DayOfWeek.Sunday)
                daysOfLastMonth = 6;

            for (DateTime lastMonth = month.AddDays(-1); daysOfLastMonth >= 0; daysOfLastMonth--, lastMonth = lastMonth.AddDays(-1))
                array[0, daysOfLastMonth] = lastMonth;

            //finish first row
            int firstRow = (int)month.DayOfWeek;
            if (month.DayOfWeek == DayOfWeek.Sunday)
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
