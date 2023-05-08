using System;
using System.Collections.Generic;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum77DataScrubber : AbstractDataScrubber
    {
        public SeqNum77DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }
        protected override bool ScrubData()
        {
            if (!ReAssignQTYtoFirstIndex() || !AddLeadZero())
            {
                return false;
            }

            return true;
        }

        private bool AddLeadZero()
        {
            try
            {
                oeRecords.ForEach(o => o.CompanyNumber = o.CompanyNumber.ToCharArray().Length == 1 ? "0" + o.CompanyNumber : o.CompanyNumber);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to reformat company number. Ex.Message: " + ex.Message;

                return false;
            }
        }
    }
}