using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller
{
    public class DefaultHDRFiller : AbstractHDRFiller
    {
        public DefaultHDRFiller(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders, filePath) { }
    }
}

