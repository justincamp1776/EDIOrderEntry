using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;

namespace REMichel.OrderEntryEDI.FileHelpers.Email
{
    public class EmailHandlerFactory
    {
        public static AbstractEmailHandler GetInstance(EDI ediRec, string fileName, EDIOEHeader order)
        {
            Type objectType = Type.GetType("OrderEntryEDI.Util.Email.SeqNum" + ediRec.seqNum + "EmailHandler");
            if (objectType != null)
            {
                return (AbstractEmailHandler)Activator.CreateInstance(objectType, ediRec, fileName, order);
            }

            return new DefaultEmailHandler(ediRec, fileName, order);
        }
    }
}
