using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class DefaultDataScrubber : AbstractDataScrubber
    {
        public DefaultDataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool ScrubData() { return true; }
    }
}
