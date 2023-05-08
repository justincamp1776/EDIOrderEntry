using REMichel.WebServicesDomain.DataServiceClients.Customer;
using REMichel.WebServicesDomain.DataServiceClients.Customer.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderFiller
{
    public class SeqNum1OrderFiller : AbstractOrderFiller
    {
        public SeqNum1OrderFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath) : base(ediRec, headers, filePath) { }

        public override List<EDIOEHeader> FillOrders()
        {
            truckRes = GetPetroShipToDetails(ediRec, filePath); //gets all petro truck/ ship to for this companyNo

            if(truckRes == null || truckRes.Count == 0)
            {
                return null;
            }
            
            foreach (EDIOEHeader hdr in headers)
            {
                string truck       = hdr.GetTruck();
                string companyCode = hdr.GetCompanyCode();

                foreach(CustomerTruck t in truckRes)
                {
                    if (truck == t.truckNumber)
                    {
                        try
                        {
                            if(!hasRequiredFields(t))
                            {
                                errMessage = "Ship_to details for truck: " + truck + " is missing required fields.";

                                throw new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, hdr, errMessage, null);
                            }

                            hdr.ShipToID            = Convert.ToInt32(t.shipToId);
                            hdr.CustomerID          = t.customerId;
                            hdr.PackingBasis        = t.packingBasis;
                            hdr.CustomerName        = t.customerName;
                            hdr.CompanyID           = t.companyID;
                            hdr.SalesLocationID     = Convert.ToInt64(t.preferredLocationID);
                            hdr.ContactID           = t.contactID;
                            hdr.ContactName         = $"{t.contactFirstName} {t.contactLastName}";
                            hdr.CustomerPONumber    = hdr.OrderDate.Replace("/", "") + "-" + truck;

                            filledOrders.Add(hdr);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            return filledOrders; 
        }

        public List<CustomerTruck> GetPetroShipToDetails(EDI ediRec, string filePath)
        {
            List<CustomerTruck> results = null;
    
            CustomerTruckService truckService = new CustomerTruckService();
            try
            {
                results = truckService.GetEDITrucks(ediRec.tradingPartnerName, filePath.Substring(filePath.Length - 6).Split('.')[0]).EntityData.resultSet;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to return all ship_to's for PETRO.";

                throw new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, null, errMessage, ex);
            }

            return results;
        }
    }
}
