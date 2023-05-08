using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum69DataScrubber : AbstractDataScrubber
    {
        public SeqNum69DataScrubber (EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            if (!ReFormatQuantity())
            {
                return false;
            }

            return true;
        }

        private bool ReFormatQuantity()
        {
            try
            {
                oeRecords.ForEach(o => o.Quantity = o.Quantity.ToString().Contains(".") ? 1 : o.Quantity);

                oeRecords.ForEach(o => o.Quantity = o.Quantity < 0 ? o.Quantity * -1 : o.Quantity);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to reformat quantity. Ex.Message: " + ex.Message;

                return false;
            }
        }
    }
}
