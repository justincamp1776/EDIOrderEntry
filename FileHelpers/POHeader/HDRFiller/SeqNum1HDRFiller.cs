using REMichel.WebServicesDomain.DataServiceClients.Customer;
using REMichel.WebServicesDomain.DataServiceClients.Customer.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller
{
    public class SeqNum1HDRFiller : AbstractHDRFiller
    {
        // gets all PETRO trucks and compares to each truck in order entry.
        // if no match then it is not a REM order.

        private List<EDIOEHeader> REMHeaders = new List<EDIOEHeader>();

        public SeqNum1HDRFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath) : base(ediRec, headers, filePath) { }

        public override List<EDIOEHeader> GetRequiredFields()
        {
            try
            {
                truckRes = GetPetroShipToDetails(ediRec, filePath); //gets all petro truck/ ship to for this companyNo
            }
            catch (Exception ex)
            {
                errMessage = ediRec.FormatExMsg(ex.Message, EDIMsgType.Internal_Error);

                throw new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, errMessage);
            }

            headers.RemoveAll(o => !truckRes.Exists(x => String.Equals(o.GetTruck(), x.truckNumber, StringComparison.OrdinalIgnoreCase)));

            headers.ForEach(o => {
                CustomerTruck t = truckRes.Find(x => String.Equals(o.GetTruck(), x.truckNumber, StringComparison.OrdinalIgnoreCase));

                try {
                    if (!hasRequiredFields(t))
                    {
                        errMessage = ediRec.FormatExMsg("Failed to return ship_to records for truck: " + t + ".", EDIMsgType.Invalid_Truck);

                        throw new Exception(errMessage);
                    }

                    o.setTruckProperties(ediRec, t);
                }
                catch (Exception ex)
                {
                    o.SetHdrDidNotPass(true);

                    o.SetExProps(new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, ex.Message));
                }
            });
           
            return headers; 
        }

        public List<CustomerTruck> GetPetroShipToDetails(EDI ediRec, string filePath)
        {
            List<CustomerTruck> results       = null;
    
            CustomerTruckService truckService = new CustomerTruckService();
           
            results = truckService.GetEDITrucks(ediRec.tradingPartnerName, filePath.Substring(filePath.Length - 6).Split('.')[0]).EntityData.resultSet;

            if (results == null || results.Count == 0)
            {
                throw new Exception("Failed to return PETRO ship_to's from Data Services.");
            }

            return results;
        }
    }
}
