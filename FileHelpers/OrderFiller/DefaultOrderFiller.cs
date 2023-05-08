using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderFiller
{
    public class DefaultOrderFiller : AbstractOrderFiller
    {
        public DefaultOrderFiller(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders, filePath) { }
    }
}

