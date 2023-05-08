using REMichel.WebServicesDomain.Types;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OrderEntryEDI.Util.Report
{
    public static class ReportUtil
    {
        public static void CreateTextFile(string fName)
        {
            string location = @"C:\edi_program_test\Log\EDI_Import_Sum";

            string fileName = fName + ".txt";

            string fPath = Path.Combine(location, fileName);

            using (new FileStream(fPath, FileMode.CreateNew)) { }
        }

        public static string BuildPath(string fName, EDIPathBuilderType t, string ext)
        {
            string rootDir = @"\\rem.remichel.com\hqdata\IT_ProcessingFiles\P21WebShare\P21PlayImports";

            string folder = GetEnumDescription(t);

            return Path.Combine(rootDir, folder, "SOH_"+fName);
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}
