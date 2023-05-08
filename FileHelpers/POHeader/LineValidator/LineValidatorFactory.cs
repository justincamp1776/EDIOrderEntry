using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.LineValidator
{
    public class LineValidatorFactory
    {
        public static AbstractLineValidator GetInstance(EDI ediRec, List<EDIOEHeader> orders)
        {
            string seqNum = ediRec.seqNum.ToString();
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.Validator.LineValidation.SeqNum" + ediRec.seqNum + "LineValidator");
            if (objectType != null)
            {
                return (AbstractLineValidator)Activator.CreateInstance(objectType, ediRec, orders);
            }

            return new DefaultLineValidator(ediRec, orders);
        }
    }
}
