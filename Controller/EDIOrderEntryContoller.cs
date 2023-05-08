using REMichel.OrderEntryEDI.FileHelpers.DataScrubber;
using REMichel.OrderEntryEDI.FileHelpers.POHeader;
using REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller;
using REMichel.OrderEntryEDI.FileHelpers.Validator;
using REMichel.OrderEntryEDI.Parser;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System.Collections.Generic;
using System.IO;

namespace REMichel.OrderEntryEDI.Controller
{
    public class EDIOrderEntryController
    {
        private EDI ediRec { get; set; }
        private string orderDate { get; set; }

        private string errMessage = "";

        public EDIOrderEntryController(EDI ediRec)
        {
            this.ediRec = ediRec;
        }

        public void BuildOrders()
        {
            List<EDIOERecord> oeRecords     = null;
            List<EDIOEHeader> headers       = null;
            string fName                    = null;

            try
            {
                // CheckServerData();

                // CheckBackupForDuplicates();

                // CheckFileExtension()

                // FileUtility.BuildPath(ediRec, filePath, EDIPathBuilderType.archive);

                IEDIParser parser = ParserFactory.GetInstance(ediRec, ediRec.filePath);
                oeRecords = parser.ReadFile(ediRec, ediRec.filePath);

                AbstractFileValidator validator = FileValidatorFactory.GetInstance(ediRec, oeRecords, ediRec.filePath);
                validator.Validate();

                AbstractDataScrubber scrubber = DataScrubberFactory.GetInstance(ediRec, oeRecords, ediRec.filePath);
                scrubber.scrubData();

                HDRBuilder hdrBuilder = new HDRBuilder(ediRec, oeRecords, ediRec.filePath);
                headers = hdrBuilder.CreateHeaders();

                // order filler fills OR flags each order
                AbstractHDRFiller orderFiller = HDRFillerFactory.GetInstance(ediRec, headers, ediRec.filePath);
                orderFiller.GetRequiredFields();

                // validates or flags each line and order
               // AbstractLineValidator lineValidator = LineValidatorFactory.GetInstance(ediRec, headers);
               // lineValidator.ValidateLines();
            }
            catch (EDIException ex)
            {
                // TODO START HERE
                ediRec.SetExFields(ex);
            }
            finally
            {
                new ApplicationOutputController(ediRec, headers, ediRec.filePath, fName).EndProcess();
            }
        }

        private void CheckServerData()
        {
            if (string.IsNullOrEmpty(ediRec.fileLocationBackup850) || string.IsNullOrEmpty(ediRec.ediFormat.ToString())
                || string.IsNullOrEmpty(ediRec.tradingPartnerName))
            {
                errMessage += ediRec.FormatExMsg("Customer EDI record is missing required fields.", EDIMsgType.Insufficient_Customer_Data);

                ediRec.hdrBuildSuccess = false;

                throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_only, errMessage);
            }
        }

        private void CheckBackupForDuplicates()
        {
            if (File.Exists(@ediRec.serverName + @ediRec.fileLocationBackup850 + Path.GetFileName(ediRec.filePath)))
            {
                errMessage += ediRec.FormatExMsg("Preexisting order. File has already been processed.", EDIMsgType.Duplicate_File);

                throw new EDIException(EDIExceptionType.DuplicateRecordException, ExceptionAlertType.rem_and_cust, errMessage);
            }
        }

        private void CheckFileExtension()
        {
            if (ediRec.filePath.Contains(".xls"))
            {
               errMessage = ediRec.FormatExMsg("Invalid Order Entry File Format. Cannot be of type .xls", EDIMsgType.Invalid_Format);

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_and_cust, errMessage);
            }
        }
    }
}