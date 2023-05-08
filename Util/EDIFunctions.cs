using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrderEntryEDI.Util
{
    public static class EDIFunctions
    {
        public static string AddSlashesToDate(string date)
        {
            if (date.Length != 6)
            {
                return date;
            }

            DateTime dt = DateTime.ParseExact(date, "MMddyy", CultureInfo.InvariantCulture);

            return dt.ToString("MM/dd/yy");
        }

        public static bool isEmptyList<T>(List<T> list)
        {
            if(list == null || list.Count == 0)
            {
                return true;
            }
            return false;
        }

        public static List<T> PopulateList<T>(List<T> sourceList, List<T> targetList)
        {
            if (isEmptyList(sourceList))
            {
                return null;
            }

            sourceList.ForEach(i => targetList.Add(i));

           return targetList;
        }
    }
}
