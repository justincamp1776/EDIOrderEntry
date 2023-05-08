using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum64DataScrubber : AbstractDataScrubber
    {
        public SeqNum64DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool ScrubData()
        {
            try
            {
                oeRecords.ForEach(o => o.Truck = String.Equals(o.Truck.ToLower(), "sp: warehouse 1") ? "1" : o.Truck);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to reassign truck. Ex.Message: " + ex.Message;

                return false;
            }
        }
    }
}