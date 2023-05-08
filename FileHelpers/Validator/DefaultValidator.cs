using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class DefaultValidator : AbstractValidator
    {
        public DefaultValidator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }
        protected override bool isNotValid() { return false; }     
    }
}
