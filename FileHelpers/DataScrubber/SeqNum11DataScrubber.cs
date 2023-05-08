using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum11DataScrubber : AbstractDataScrubber
    {
        public SeqNum11DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath) : base(ediRec, oeRecords, filePath) { }

        protected override bool ScrubData()
        {
            try
            {
                oeRecords.ForEach(o => o.Truck = Regex.Replace(o.Truck, @"\(.*?\)", "")); 
               
                return true;
            }
            catch
            {
                errMessage = "Failed to remove parenthesis from truck.";

                return false;
            }
        }
    }
}