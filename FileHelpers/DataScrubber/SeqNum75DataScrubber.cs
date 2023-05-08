using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum75DataScrubber : AbstractDataScrubber
    {
        public SeqNum75DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

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
