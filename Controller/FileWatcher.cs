using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.DataServiceClients.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.Controller
{
    public class EDIFileWatcher
    {
        private List<EDI> ediCustomers = null;

        private IDictionary<string, List<EDI>> custDictionary = null;

        public EDIFileWatcher() { }


        public void InitFileWatcher()
        {
            EDIService ediService = new EDIService();
            ediCustomers = ediService.Get().EntityData.resultSet;

            //List<string> dirs = ediCustomers.Select(e => e.serverName + e.fileLocation850).ToList();
            custDictionary = ediCustomers.GroupBy(c => c.serverName + c.fileLocation850).ToDictionary(x => x.Key, x => x.ToList());

            foreach (string key in custDictionary.Keys)
            {
                FileSystemWatcher fileSysetmWatcher = new FileSystemWatcher();
                fileSysetmWatcher.Path = key;
                fileSysetmWatcher.Filter = "*.*";
                fileSysetmWatcher.IncludeSubdirectories = false;
                fileSysetmWatcher.Created += new FileSystemEventHandler(OnCreated);
                fileSysetmWatcher.EnableRaisingEvents = true;
                fileSysetmWatcher.NotifyFilter = NotifyFilters.FileName;

            }
        }

 
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
            {
                return;
            }

            try
            {
                Thread.Sleep(2000);

                EDI ediRec = custDictionary[Path.GetDirectoryName(e.FullPath) + "\\"].Single();

                ediRec.filePath = e.FullPath;

                new EDIOrderEntryController(ediRec).BuildOrders();
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Console.WriteLine(string.Format("Error On File {0} Deletion On {1} :: {2} ", DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss"), e.FullPath, ex.Message));

                SendFailureEmail(ex);
            }
        }

        private bool SendFailureEmail(Exception ex)
        {
            string subj = "CRITICAL: FTP EDI PROGRAM FAILURE";
            Lib.Email.EmailMessage em = new Lib.Email.EmailMessage(subj, true);

            em.AddToAddress("justin.campbell@remichel.com");
            em.AddMsgBody(BuildMsgBody(ex));

            ServiceResult<bool> task = Task.Run(async () =>
            {
                ServicesService sService = new ServicesService();

                return await sService.SendEmail(em);

            }).Result;

            return true;
        }

        private string BuildMsgBody(Exception ex)
        {
            bool hasInner;

            hasInner = ex.InnerException == null ? false : true;

            StringBuilder sb = new StringBuilder();

            sb.Append("***This is a critical message.***");
            sb.Append("\n\n");
            sb.Append("An FTP EDI Program has failed. The exception has bubbled up to the FileWatcher class.");
            sb.Append("\n\n");
            sb.Append("See Below: ");
            sb.Append("\n\n");
            sb.Append(hasInner ?ex.InnerException.Message:ex.Message);
            sb.Append("\n\n");
            sb.Append(hasInner ? ex.InnerException.Source.ToString(): ex.Source.ToString());
            sb.Append("\n\n");
            sb.Append(hasInner ?ex.InnerException.TargetSite.ToString():ex.TargetSite.ToString());


            return sb.ToString();
        }
    }
}
