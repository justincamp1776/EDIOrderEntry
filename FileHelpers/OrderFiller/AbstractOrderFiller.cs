using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.Customer;
using REMichel.WebServicesDomain.DataServiceClients.Customer.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderFiller
{
    //TODO create default Taker for EDI

    public abstract class AbstractOrderFiller
    {
        protected EDI ediRec                { get; set; }
        protected string filePath           { get; set; }
        protected List<EDIOEHeader> headers  { get; set; }

        protected string errMessage                 = null;
        protected List<CustomerTruck> truckRes      = null;
        protected List<EDIOEHeader> filledOrders    = new List<EDIOEHeader>();

        public AbstractOrderFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath)
        {
            this.ediRec     = ediRec;
            this.headers    = headers;
            this.filePath   = filePath;
        }

        public virtual List<EDIOEHeader> FillOrders()
        {
            foreach (EDIOEHeader hdr in headers)
            {
                try
                {
                    GetShipToDetails(hdr);
                }
                
                catch (Exception)
                {
                    throw;

                    //hdr.SetisNotValid(true);
                    //ediRec.AddExToList(ExceptionType.System);
                    //continue;
                }
            }

           // headers.Where(h => h.IsNotValid() == false).ToList().ForEach(h => filledOrders.Add(h));

            return filledOrders;
        }
       
        protected void GetShipToDetails(EDIOEHeader hdr)
        {
            ServiceResult<CustomerTruck> res = null;

            string truck           = hdr.GetTruck();
            string companyCode     = hdr.GetCompanyCode();
            decimal customerID     = ediRec.seqNum == 77? hdr.CustomerID:0;

            res = Task.Run(async () =>
            {
                CustomerTruckService truckService = new CustomerTruckService();

                return await truckService.GetTruck(truck, ediRec.tradingPartnerName, companyCode, Convert.ToInt64(customerID));

            }).Result;


            if (res.EntityData == null || !res.isOk() || !hasRequiredFields(res.EntityData))
            {
                //hdr.SetisNotValid(true);

                errMessage = "Failed to return ship_to records for truck: " + truck + ".";

                throw new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, hdr, errMessage, null);
            }


            hdr.ShipToID        = Convert.ToInt32(res.EntityData.shipToId);
            hdr.CustomerID      = Convert.ToDecimal(res.EntityData.customerId);
            hdr.PackingBasis    = res.EntityData.packingBasis;
            hdr.CustomerName    = res.EntityData.customerName;
            hdr.CompanyID       = res.EntityData.companyID;
            hdr.SalesLocationID = Convert.ToInt64(res.EntityData.preferredLocationID);
            hdr.ContactID       = res.EntityData.contactID;
            hdr.ContactName     = $"{res.EntityData.contactFirstName} {res.EntityData.contactLastName}";

            filledOrders.Add(hdr);
        }

        protected bool hasRequiredFields(CustomerTruck res)
        {
            if(string.IsNullOrEmpty(res.shipToId.ToString()) || res.shipToId == 0 
                || string.IsNullOrEmpty(res.customerId.ToString()) || res.customerId == 0
                || string.IsNullOrEmpty(res.preferredLocationID.ToString()) || res.preferredLocationID == 0
                || string.IsNullOrEmpty(res.contactID) 
                || string.IsNullOrEmpty(res.contactFirstName) 
                || string.IsNullOrEmpty(res.contactLastName)){

                return false;
            }

            return true;
        }
    }
}
