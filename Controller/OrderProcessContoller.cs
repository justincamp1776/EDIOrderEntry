
using REMichel.OrderEntryEDI.FileHelpers.CustomerAudit;
using REMichel.OrderEntryEDI.FileHelpers.DataScrubber;
using REMichel.OrderEntryEDI.FileHelpers.OEHeaderBuilder;
using REMichel.OrderEntryEDI.FileHelpers.OrderFiller;
using REMichel.OrderEntryEDI.FileHelpers.Validator.LineValidation;
using REMichel.OrderEntryEDI.FileHelpers.OrderWriter;
using REMichel.OrderEntryEDI.FileHelpers.Validator;
using REMichel.OrderEntryEDI.Parser;
using REMichel.WebServicesDomain.DataServiceClients.CodeTable;
using REMichel.WebServicesDomain.DataServiceClients.CodeTable.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OrderEntryEDI.Util;

namespace REMichel.OrderEntryEDI.Controller
{
    public class OrderProcessContoller
    {
        private EDI ediRec                      { get; set; }
        private string filePath                 { get; set; }
        private string orderDate                { get; set; }

        private string errMessage               = null;

        private readonly List<Exception> exceps = new List<Exception>();


        public OrderProcessContoller(EDI ediRec, string filePath)
        {
            this.ediRec = ediRec;
            this.filePath = filePath;
        }


        public void ProcessOrders()
        {
            List<EDIOERecord> oeRecords     = null;
            List<EDIOEHeader> headers       = null;
            List<EDIOEHeader> filledOrders  = null;
            string P21ImportDir             = null;
            string tempDir                  = null;
            string fName                    = null;
            string orderDate                = null;

            try
            {
              

                CheckServerData();

                //CheckBackupForDuplicates();

                //FileUtility.BuildPath(ediRec, filePath, EDIPathBuilderType.archive);

                IEDIParser parser = ParserFactory.GetInstance(ediRec, filePath);
                oeRecords = parser.ReadFile(ediRec, filePath);

                orderDate = EDIFunctions.AddSlashesToDate(oeRecords.Select(r => r.OrderDate).FirstOrDefault());

                AbstractValidator validator = ValidatorFactory.GetInstance(ediRec, oeRecords, filePath);
                validator.Validate();

                AbstractDataScrubber scrubber = DataScrubberFactory.GetInstance(ediRec, oeRecords, filePath);
                scrubber.DataScrubber();

                OEHeaderBuilder hdrBuilder = new OEHeaderBuilder(ediRec, oeRecords, filePath);
                headers = hdrBuilder.CreateHeaders();

                orderDate = oeRecords.Select(r => r.OrderDate).FirstOrDefault();

                AbstractOrderFiller orderFiller = OrderFillerFactory.GetInstance(ediRec, headers, filePath);
                filledOrders = orderFiller.FillOrders();

                if (!hasRecords(filledOrders))
                {
                    return;
                }

                //Need Data Services DAO
                AbstractLineValidator lineValidator = LineValidatorFactory.GetInstance(ediRec, filledOrders, filePath);
                lineValidator.ValidateLines();

                AbstractCustomerAudit customerAudit = CustomerAuditFactory.GetInstance(ediRec, filledOrders, filePath);
                customerAudit.CreateAuditTable();

                SystemPropertyService sps = new SystemPropertyService();
                List<SystemProperty> sysProps = sps.Get().EntityData.resultSet;

                tempDir = sysProps.Single(x => x.sysKey == SystemPropertyKey.EDI_P21_OE_TEMP_DIR.ToString()).value;
                P21ImportDir = sysProps.Single(x => x.sysKey == SystemPropertyKey.EDI_P21_OE_DIR.ToString()).value;

                fName = ediRec.tradingPartnerName + DateTime.Now.ToString("mmssffffff") + ".txt";

                new OrderWriter().WriteOrders(filledOrders, ediRec, filePath, tempDir, fName);

            }

            catch (EDIException e)
            {
                Debug.WriteLine("Ex.Message: "+e.Message);

                ediRec.AddExToList(e);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ex.Message: "+ex.Message);

                ediRec.AddExToList(new EDIException(EDIExceptionType.SystemException, ExceptionAlertType.rem_only, null, ex.Message, ex));
            }
            finally
            {
                EndOFProcessController endProcessController = new EndOFProcessController(ediRec, filledOrders, filePath, orderDate, fName);
                endProcessController.EndProcess(P21ImportDir, tempDir);

                // new ReportMain(fName).CheckImportResults();
            }
        }
    
        private bool hasRecords(List<EDIOEHeader> orders)
        {
            if (orders == null || orders.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void CheckServerData()
        {
            int one = 1;
            if (string.IsNullOrEmpty(ediRec.fileLocation850) || string.IsNullOrEmpty(ediRec.fileLocationBackup850) || string.IsNullOrEmpty(ediRec.serverName)
                || string.IsNullOrEmpty(ediRec.tradingPartnerName)) {

                errMessage = "EDI DTO is missing required properties.";

                throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_only, null, errMessage, null);
            }
        }

        private void CheckBackupForDuplicates()
        {
            if (File.Exists(@ediRec.serverName + @ediRec.fileLocationBackup850 + Path.GetFileName(filePath)))
            {
                errMessage = "Preexisting order. File has already been processed.";

                throw new EDIException(EDIExceptionType.DuplicateRecordException, ExceptionAlertType.cust_only, null, errMessage, null);
            }
        }
    }
}