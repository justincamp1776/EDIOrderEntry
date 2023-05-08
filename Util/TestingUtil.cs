using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OrderEntryEDI.Util
{
    public class TestingUtil
    {
        private List<EDI> ediList { get; set; }
        private List<long> seqNums { get; set; }


        public TestingUtil()
        {
            ediList = GetRecords();
        }

        public TestingUtil(List<long> seqNums)
        {
            ediList = GetRecords();
            this.seqNums = seqNums;
        }

        private List<EDI> GetRecords()
        {
            EDIService ediService = new EDIService();
            return ediList = ediService.Get().EntityData.resultSet;
        }

        public void CopySelectFilesForTest()
        {
            foreach (EDI rec in ediList)
            {
                foreach (int s in seqNums)
                {
                    if (rec.seqNum == s)
                    {
                        List<string> allFiles = Directory.GetFiles(rec.serverName + rec.fileLocationBackup850).ToList();

                        if (allFiles == null || allFiles.Count == 0)
                        {
                            continue;
                        }

                        Copyfiles(allFiles, rec);
                    }
                }
            }  
        }

        public void CopyAllCustomerFilesForTest()
        {
            foreach (EDI rec in ediList)
            {
                if(rec.seqNum == 36)
                {
                    continue;
                }

                List<string> allFiles = Directory.GetFiles(rec.serverName + rec.fileLocationBackup850).ToList();

                if (allFiles == null || allFiles.Count == 0)
                {
                    continue;
                }

                Copyfiles(allFiles, rec);
            }
        }

        private void Copyfiles(List<string> allFiles, EDI rec)
        {
            foreach (string fPath in allFiles)
            {
                string fileName = Path.GetFileName(fPath);



                File.Copy(fPath, Path.Combine(rec.serverName, rec.fileLocation850.TrimStart('\\'),fileName));
            }
        }

        public void DeleteFilesInSelectPOs()
        {
            foreach (EDI rec in ediList)
            {
                foreach (int s in seqNums)
                {
                    if (rec.seqNum == s)
                    {
                        DeleteFiles(rec);
                    }
                }
            }
        }

        public void DeleteFilesInALLPOs()
        {
            foreach (EDI rec in ediList)
            {
                DeleteFiles(rec);
            }
        }

        private void DeleteFiles(EDI rec)
        {
            if(rec.seqNum == 11)
            {
                string o = "";
            }

            DirectoryInfo di = new DirectoryInfo(@rec.serverName + rec.fileLocation850);

            List<FileInfo> allFiles = di.GetFiles().ToList();
            allFiles.ForEach(f => f.Delete());
        }
    }
}
