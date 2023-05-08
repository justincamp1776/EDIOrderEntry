using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum34DataScrubber : AbstractDataScrubber
    {
        public SeqNum34DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {    
            try
            {
                oeRecords.ForEach(o => o.CompanyNumber = o.Truck.Substring(0, 2).ToUpper() == "EG" ? "02" : o.CompanyNumber);
                oeRecords.ForEach(o => o.CompanyNumber = o.Truck.Substring(0, 2).ToUpper() == "BT" ? "03" : o.CompanyNumber);
                oeRecords.ForEach(o => o.CompanyNumber = o.Truck.Substring(0, 2).ToUpper() != "BT" && o.Truck.Substring(0, 2).ToUpper() != "EG" ? "01" : o.CompanyNumber);

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



            