using REMichel.OrderEntryEDI.FileHelpers.Validator;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator
{
    public class SeqNum78FileValidator : AbstractFileValidator
    {
        public SeqNum78FileValidator(EDI ediRec, List<EDIOERecord> oeRecords, string absolutePath) : base(ediRec, oeRecords, absolutePath) { }

        protected override bool isNotValid()
        {
            if (oeRecords.Select(r => r.Quantity < 0).First())
            {
                return false;
            }

            bool isValid = false;

            oeRecords.ForEach(r =>
            {
                if(!string.IsNullOrEmpty(r.ItemID))
                {
                    if (string.IsNullOrEmpty(r.OrderDate)
                    || string.IsNullOrEmpty(r.CustomerPONumber)
                    || string.IsNullOrEmpty(r.Truck)
                    || string.IsNullOrEmpty(r.Quantity.ToString()))
                    {
                        errMessage = String.Format("An {0} order line is missing one or more required values.", ediRec.tradingPartnerName);

                        isValid = true;
                    }
                }
            });

            return isValid;
        }
    }
}
