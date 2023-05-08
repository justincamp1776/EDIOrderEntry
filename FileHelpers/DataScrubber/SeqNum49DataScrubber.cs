using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum49DataScrubber : AbstractDataScrubber
    {
        public SeqNum49DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            if (!ReplaceTruckTilde())
            {
                return false;
            }

            return true;
        }
    }
}
