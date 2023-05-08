using REMichel.OrderEntryEDI.FileHelpers.DataScrubber;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum73DataScrubber : AbstractDataScrubber
    {
        public SeqNum73DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

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
