using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class SeqNum1FileValidator : AbstractFileValidator
    {
        // Petro OrderFiller (SeqNum1OrderFiller) requires company number to Get required fields from p21 DB
        public SeqNum1FileValidator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool isNotValid()
        {
            return false;
        }
    }
}
