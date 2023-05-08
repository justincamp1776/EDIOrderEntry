using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.CustomerAudit
{
    public class CustomerAuditFactory
    {
        public static AbstractCustomerAudit GetInstance(EDI ediRec, List<EDIOEHeader> orders, string filePath)
        {
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.CustomerAudit.SeqNum" + ediRec.seqNum + "CustomerAudit");
            if (objectType != null)
            {
                return (AbstractCustomerAudit)Activator.CreateInstance(objectType, ediRec, orders, filePath);
            }
            return new DefaultCustomerAudit(ediRec, orders, filePath);
        }
    }
}
