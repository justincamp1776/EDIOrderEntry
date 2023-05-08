using System;
using System.Collections.Generic;

using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderFiller
{
    public class OrderFillerFactory
    {
        public static AbstractOrderFiller GetInstance(EDI custRecord, List<EDIOEHeader> orders, string filePath)
        {
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.OrderFiller.SeqNum" + custRecord.seqNum + "OrderFiller");
            if (objectType != null)
            {
                return (AbstractOrderFiller)Activator.CreateInstance(objectType, custRecord, orders, filePath);
            }

            return new DefaultOrderFiller(custRecord, orders, filePath);
        }
    }
}