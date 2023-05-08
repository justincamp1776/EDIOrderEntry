using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller
{
    public class SeqNum77HDRFiller : AbstractHDRFiller
    {
        public SeqNum77HDRFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath) : base(ediRec, headers, filePath) { }

        public override List<EDIOEHeader> GetRequiredFields()
        {
            foreach (EDIOEHeader hdr in headers)
            {
                try
                {
                    EDIMichaelSonService ediService  = new EDIMichaelSonService();

                    ServiceResult<EDIMichaelSon> res = ediService.GetCustomerID(hdr.GetCompanyCode());

                    if (res == null || res.EntityData == null || !res.isOk() || res.EntityData.accountNumber == 0)
                    {
                        errMessage = ediRec.FormatExMsg("Failed to return ship_to records for truck: " + hdr.GetTruck() + ".", EDIMsgType.Invalid_Truck);

                        throw new Exception(errMessage);
                    }

                    hdr.customerID = Convert.ToDecimal(res.EntityData.accountNumber);

                }
                catch (Exception ex)
                {
                    hdr.SetHdrDidNotPass(true);

                    hdr.SetExProps(new EDIException(EDIExceptionType.MichaelSonServiceException, ExceptionAlertType.rem_only, ex.Message));

                    continue;
                }
            }

            return GetCustomerRecord(); 
        }
    }
}
