using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public abstract class AbstractFileValidator
    {
        protected EDI ediRec                    { get; set; }
        protected string filePath               { get; set; }
        protected List<EDIOERecord> oeRecords   { get; set; }

        protected string errMessage = null;

        protected AbstractFileValidator(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) 
        {
            this.oeRecords = oeRecords;
            this.filePath  = filePath;
            this.ediRec    = ediRec;
        }

        public void Validate()
        {
            try
            {
                if (isNotValid())
                {
                    throw new Exception(errMessage);
                }
            }
            catch (Exception ex)
            {
                string errMsg = errMessage == null ? ex.Message : ediRec.FormatExMsg(errMessage, EDIMsgType.Invalid_Format);

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_and_cust, errMsg);
            }
        }

        protected abstract bool isNotValid();

        protected bool hasEmptyLines()
        {
            if (oeRecords.Select(o => string.IsNullOrEmpty(o.GetPropsAsString())).First())
            {
                errMessage = "OERecord contains an empty line.";

                return true;
            }
            return false;
        }

        protected bool isValidDate()
        {
            if(oeRecords.Select(o => o.OrderDate.Contains("/") 
                                    || o.OrderDate.Contains("-") 
                                    || o.OrderDate.Contains("''")
                                    || o.OrderDate.Contains("Document Date")).First())
            {
                errMessage = "Detected invalid chars in order date.";

                return false;
            }

            return true;
        }     
    }
}
