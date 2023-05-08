using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.LineValidator
{
    public class DefaultLineValidator : AbstractLineValidator
    {
        public DefaultLineValidator(EDI ediRec, List<EDIOEHeader> orders) : base(ediRec, orders) { }
    }
}
