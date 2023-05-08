using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrderEntryEDI.Util
{
    public static class FileWriterUtil
    {
        public static void WriteTextFile(string fName, List<string> list, Dictionary<string,string> lastWrites)
        {
            string path = "";
            string aPath = "";
            string bPath = "";
            path += @"C:\Justin\edi\txt\ediHistory\";
            
            
            if(lastWrites != null)
            {
                aPath += @"C:\Justin\edi\txt\ediHistory\names";


                using (StreamWriter sw = File.CreateText(aPath))
                {
                    foreach(var l in lastWrites)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(l.Key);
                 

                        sw.WriteLine(sb.ToString());
                    }
                }

                bPath += @"C:\Justin\edi\txt\ediHistory\dates";


                using (StreamWriter sw = File.CreateText(bPath))
                {
                    foreach (var l in lastWrites)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(l.Value);


                        sw.WriteLine(sb.ToString());
                    }
                }

            }
            else
            {
                path += fName;

                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        list.ForEach(l =>
                        {
                            sw.WriteLine(l);

                        });
                    }
                }
            }
        }
    }
}
