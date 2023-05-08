using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderFiller
{
    public class SeqNum77OrderFiller : AbstractOrderFiller
    {
        public SeqNum77OrderFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath) : base(ediRec, headers, filePath) { }

        public override List<EDIOEHeader> FillOrders()
        {
            foreach (EDIOEHeader hdr in headers)
            {
                try
                {
                    EDIMichaelSonService ediService = new EDIMichaelSonService();

                    ServiceResult<EDIMichaelSon> res = ediService.GetCustomerID(hdr.GetCompanyCode());

                    if (res.EntityData == null || !res.isOk() || res.EntityData.accountNumber == 0)
                    {
                        errMessage = "No ship_to records were returned for truck: " + hdr.GetTruck() + ".";

                        throw new EDIException(EDIExceptionType.MichaelSonServiceException, ExceptionAlertType.rem_only, hdr, errMessage, null);
                    }

                    hdr.CustomerID = Convert.ToDecimal(res.EntityData.accountNumber);

                    GetShipToDetails(hdr);
                }
                catch (Exception)
                {
                    //hdr.SetisNotValid(true);
                    //ediRec.AddExToList(ExceptionType.System);
                    //continue;

                    throw;
                }
            }

           // headers.Where(h => h.IsNotValid() == false).ToList().ForEach(h => filledOrders.Add(h));

            return filledOrders;
        }
    }
}
