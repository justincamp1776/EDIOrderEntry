using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator.LineValidation
{
    public class DefaultLineValidator : AbstractLineValidator
    {
        public DefaultLineValidator(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders, filePath) { }
    }
}
