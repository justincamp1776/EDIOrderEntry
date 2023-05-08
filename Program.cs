using OrderEntryEDI.Util;
using REMichel.OrderEntryEDI.Controller;
using REMichel.WebServicesDomain.Config;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using System.Collections.Generic;
using System.Configuration;

namespace OrderEntryEDI
{
    public class Program
    {
        private static void InitConfig()
        {
            ServicesConfig.REM_APP_SYSTEM_IDENTITY  = ConfigurationManager.AppSettings["REMAppSystemIdentity"];
            ServicesConfig.REM_DATASERVICE_BASE_URL = ConfigurationManager.AppSettings["REMDataServiceBaseURL"];
        }

        static void Main(string[] args)
        {
            InitConfig();

            List<long> seqNums = new List<long> { 1 };

            TestingUtil testUtil = new TestingUtil(seqNums); 

            testUtil.DeleteFilesInSelectPOs();

            //With Q & Timer
            //EDIFileWatcher ediFileWatcher = new EDIFileWatcher();
            //ediFileWatcher.InitFileWatcher();

            EDIFileWatcher ediFileWatcher = new EDIFileWatcher();

            ediFileWatcher.InitFileWatcher();

            testUtil.CopySelectFilesForTest();

            
           /* EDIService ediService = new EDIService();

            var ediCustomers = ediService.Get().EntityData.resultSet;

            var lastWrites = LastWriteUtil.GetLastWrite(ediCustomers);

            FileWriterUtil.WriteTextFile("ediCreationHistory.txt", null, lastWrites);*/
           

            while (1 == 1)
            {

            }
        }
    }
}
