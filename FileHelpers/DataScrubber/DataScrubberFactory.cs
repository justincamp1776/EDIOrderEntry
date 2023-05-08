
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public class DataScrubberFactory 
    {
        public static AbstractDataScrubber GetInstance(EDI rec, List<EDIOERecord> oeRecords, string absolutePath)
        {
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.DataScrubber.SeqNum" + rec.seqNum + "DataScrubber");
            if (objectType != null)
            {
                return (AbstractDataScrubber)Activator.CreateInstance(objectType, rec, oeRecords, absolutePath);
            }
            return new DefaultDataScrubber(rec, oeRecords, absolutePath);
        }
    }
}