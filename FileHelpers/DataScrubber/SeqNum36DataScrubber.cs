using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum36DataScrubber : AbstractDataScrubber
    {
        public SeqNum36DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool ScrubData()
        {
            try
            {
                oeRecords.ForEach(o => o.CompanyNumber = "02");

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