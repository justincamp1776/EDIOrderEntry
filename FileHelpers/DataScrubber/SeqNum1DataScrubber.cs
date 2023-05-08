using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum1DataScrubber : AbstractDataScrubber
    {
        public SeqNum1DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            // VB6 skips lines with QTY <= 0
            oeRecords.RemoveAll(o => o.Quantity <= 0 );

            if (!ReAssignQTYtoOne())
            {
                return false;
            }

            return true;
        }
    }
}

            