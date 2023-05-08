using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class DefaultFileValidator : AbstractFileValidator
    {
        public DefaultFileValidator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }
        protected override bool isNotValid()
        {
            if (!isValidDate() || hasEmptyLines())
            {
                return true;
            }

            return false;
        }     
    }
}

