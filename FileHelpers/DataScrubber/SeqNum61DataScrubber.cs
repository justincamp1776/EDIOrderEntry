using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum61DataScrubber : AbstractDataScrubber
    {
        public SeqNum61DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
           if (!ReAssignQTYtoFirstIndex())
           {
              return false;
           }

            return true;
        }
    }
}
