using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OrderEntryEDI.Util
{
    static class LastWriteUtil
    {
        public static Dictionary<string, string> GetLastWrite(List<EDI> allCustomers)
        {
            Dictionary<string, string> lastWrites = new Dictionary<string, string>(); ;
            List<string> emptyDirs = new List<string>();
            List<string> outboundOnly = new List<string>();
            

            allCustomers.ForEach(c =>
            {
                if (c.seqNum != 45 )
                {
                    string rootDir = "";
                    string fName = "";
                    string date = "";
                    string[] filePaths = null;

                    string server = "\\\\remftp1";

                    rootDir += server;
                    rootDir += c.fileLocationBackup850;

                    if (Directory.Exists(@rootDir))
                    {
                        filePaths = Directory.GetFiles(@rootDir);
                    }

                    if (filePaths != null && filePaths.Length > 0)
                    {
                        var directory = new DirectoryInfo(@rootDir);
                        var myFile = (from f in directory.GetFiles("*.*")
                                      where !Path.GetFileName(f.FullName).Contains("810")
                                      orderby f.LastWriteTime descending
                                      select f).FirstOrDefault();

                        if (myFile != null)
                        {
                            string testServer = @"C:\edi_program_test";

                            string target = testServer += c.fileLocationBackup850;

                            string fName2 = Path.GetFileName(myFile.FullName);

                            if(!Directory.Exists(target += fName2))
                            {
                                File.Copy(myFile.FullName, target += fName2);
                            }
                           
                            //fName += Path.GetFileName(myFile.FullName);
                            //date += myFile.CreationTime.ToShortDateString();
                            //lastWrites.Add(c.tradingPartnerName, date); 
                        }
                    }
                    if (filePaths != null && filePaths.Length > 0 && filePaths.All(f => f.Contains("810")))
                    {
                        outboundOnly.Add(rootDir);
                    }
                    else
                    {
                        emptyDirs.Add(rootDir);
                    }
                }
                
            });

            FileWriterUtil.WriteTextFile("emptyEdiDirs.txt", emptyDirs, null);
            FileWriterUtil.WriteTextFile("ftpOutbound810Only.txt", outboundOnly, null);

            return lastWrites;
        }

        private static string FormatFileInfo(Dictionary<string, string> lastWrites)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var i in lastWrites)
            {
                sb.Append("FileName: ");
                sb.Append(i.Key);
                sb.Append("               ");
                sb.Append("Last Submission: ");
                sb.Append(i.Value);
                sb.Append("/n/n");
            }
   
            return sb.ToString();
        }
    }
}
