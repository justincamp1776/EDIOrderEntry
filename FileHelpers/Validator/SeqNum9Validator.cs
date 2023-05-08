using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class SeqNum9Validator : AbstractValidator
    {
        // checking for common issues recorded in vb6 program
        public SeqNum9Validator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }
       
        protected override bool isNotValid()
        {
            if (oeRecords.Select(o => o.OrderDate.Contains("Document Date")).First())
            {
                errMessage = "Order Date cannot contain header.";

                return true;
            }
            if (oeRecords.Select(o => o.ItemID.Contains("E+")).First())
            {
                errMessage = "Item ID was found containing scientific notation.";

                return true;
                
            }
            return false;
        }
    }  
}
