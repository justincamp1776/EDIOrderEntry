using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class SeqNum78DataScrubber : AbstractDataScrubber
    {
        public SeqNum78DataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }
        protected override bool ScrubData()
        {
            try
            {
                oeRecords.ForEach(r =>
                {
                    if (!string.IsNullOrEmpty(r.ItemID))
                    {
                        if (r.ItemID.Contains("+"))
                        {
                            string temp = r.ItemID.Replace("+", "0");

                            r.ItemID = temp;
                        } 
                    }
                });

                return true;
            }
            catch(Exception ex)
            {
                string msg = "Unable to Reformat an ItemID that contained scientific notation.";

                errMessage = ediRec.FormatExMsg(msg, ex);

                return false;
            }   
        }
    }
}
