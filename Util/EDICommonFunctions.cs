using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Remichel.OrderEntryEDI.Util
{
    public static class EDICommonFunctions
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
            return list == null || list.Count == 0;
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

        public static bool orderHasOneCompanyNo(List<EDIOEHeader> orders)
        {
            foreach(EDIOEHeader o in orders)
            {
               string firstCompanyCode = o.GetCompanyCode();

               if(o.oeLine.Select(x => x.GetCompanyCode() != firstCompanyCode).FirstOrDefault())
               {
                    return false;
               }
            }

            return true;
        }
    }
}
