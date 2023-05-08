using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class SeqNum77Validator : AbstractValidator
    {
        // MichaelSon OrderFiller (SeqNum77OrderFiller) requires company number to Get required fields from p21 DB
        public SeqNum77Validator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool isNotValid()
        {
            if (hasEmptyCompanyNo())
            {
                return true;
            }

            return false;
        }
    }
}
