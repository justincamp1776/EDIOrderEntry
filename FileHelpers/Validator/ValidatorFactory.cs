using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;


namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class FileValidatorFactory
    {
        public static AbstractFileValidator GetInstance(EDI rec, List<EDIOERecord> oeRecords, string filePath)
        {
            string seqNum = rec.seqNum.ToString();
            Type objectType = Type.GetType("REMichel.OrderEntryEDI.FileHelpers.Validator.SeqNum" + seqNum + "FileValidator");
            if (objectType != null)
            {
                return (AbstractFileValidator)Activator.CreateInstance(objectType, rec, oeRecords, filePath);
            }
            return new DefaultFileValidator(rec, oeRecords, filePath);
        }
    }
}
