using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System.Collections.Generic;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public abstract class AbstractValidator
    {
        protected EDI ediRec                    { get; set; }
        protected string filePath               { get; set; }
        protected List<EDIOERecord> oeRecords   { get; set; }

        protected string errMessage = null;

        protected AbstractValidator(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) 
        {
            this.oeRecords = oeRecords;
            this.filePath  = filePath;
            this.ediRec    = ediRec;
        }

        public void Validate()
        {
            if(hasEmptyLines() || hasEmptyTrucks() || isNotValid())
            {
               throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_and_cust, null, errMessage, null);
            }
        }

        protected abstract bool isNotValid();

        private bool hasEmptyLines()
        {
            // CSV Parser will throw exception if any lines are empty
            if (ediRec.ediFormat != EDIFormat.X12)
            {
                return false;
            }

            if (oeRecords.Select(o => string.IsNullOrEmpty(o.GetPropsAsString())).First())
            {
                errMessage = "OERecord contains an empty line.";

                return true;
            }
            return false;
        }

        private bool hasEmptyTrucks()
        {
            if (oeRecords.Select(o => string.IsNullOrEmpty(o.Truck)).First())
            {
                errMessage = "A quote line was submitted without a truck.";

                return true;
            }
            return false;
        }
        
        protected bool hasEmptyCompanyNo()
        {
            if (oeRecords.Select(o => string.IsNullOrEmpty(o.CompanyNumber) || string.IsNullOrWhiteSpace(o.CompanyNumber)).First())
            {
                errMessage = "A quote line was submitted without a company number.";

                return true;
            }
            return false;
        }
    }
}
