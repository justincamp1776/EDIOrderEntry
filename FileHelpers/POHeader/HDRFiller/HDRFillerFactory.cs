using System;
using System.Collections.Generic;

using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller
{
    public class HDRFillerFactory
    {
        public static AbstractHDRFiller GetInstance(EDI custRecord, List<EDIOEHeader> orders, string filePath)
        {
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller.SeqNum" + custRecord.seqNum + "HDRFiller");
            if (objectType != null)
            {
                return (AbstractHDRFiller)Activator.CreateInstance(objectType, custRecord, orders, filePath);
            }

            return new DefaultHDRFiller(custRecord, orders, filePath);
        }
    }
}