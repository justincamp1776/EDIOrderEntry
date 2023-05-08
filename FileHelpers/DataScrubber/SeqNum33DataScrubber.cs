using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum33DataScrubber : AbstractDataScrubber
    {
        public SeqNum33DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            if (!AssignCompanyNo())
            {
                return false;
            }

            return true;
        }

        private bool AssignCompanyNo()
        {
            try
            {
                oeRecords.ForEach(o => o.CompanyNumber = "01");

                return true;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to reassign company number. Ex.Message: " + ex.Message;

                return false;
            }
        }
    }
}
