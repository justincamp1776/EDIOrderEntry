using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum52DataScrubber : AbstractDataScrubber
    {
        public SeqNum52DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            if (!ReAssignQTYtoOne())
            {
                return false;
            }

            return true;
        }
    }
}
