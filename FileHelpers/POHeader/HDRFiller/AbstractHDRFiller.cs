using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.Customer;
using REMichel.WebServicesDomain.DataServiceClients.Customer.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.HDRFiller
{
    public abstract class AbstractHDRFiller
    {
        protected EDI ediRec                    { get; set; }
        protected string filePath               { get; set; }
        protected List<EDIOEHeader> headers     { get; set; }

        protected string errMessage                     = null;
        protected List<CustomerTruck> truckRes          = null;
        protected List<EDIOEHeader> filledOrders        = null;

        public AbstractHDRFiller(EDI ediRec, List<EDIOEHeader> headers, string filePath)
        {
            this.ediRec     = ediRec;
            this.headers    = headers;
            this.filePath   = filePath;
        }


        //TODO ADD THE MSG TYPES TO EXCEPTIONS

        public virtual List<EDIOEHeader> GetRequiredFields()
        {
            if (headers == null || headers.Count == 0)
            {
                throw new Exception("Headers are null or empty");
            }

            return GetCustomerRecord();
        }
       
       protected List<EDIOEHeader> GetCustomerRecord()
       {
            filledOrders = new List<EDIOEHeader>();

            headers.ForEach(hdr =>
            {
                try
                {
                    ServiceResult<CustomerTruck> res = null;

                    string truck = hdr.GetTruck();

                    string companyCode = hdr.GetCompanyCode();

                    decimal customerID = ediRec.seqNum == 77 ? hdr.customerID : 0;

                    res = Task.Run(async () =>
                    {
                        CustomerTruckService truckService = new CustomerTruckService();

                        return await truckService.GetTruck(truck, ediRec.tradingPartnerName, companyCode, Convert.ToInt64(customerID));

                    }).Result;

                    if (res.EntityData == null || !res.isOk() || !hasRequiredFields(res.EntityData))
                    {
                        errMessage = ediRec.FormatExMsg("Failed to return ship_to records for truck: " + truck + ".", EDIMsgType.Invalid_Truck);

                        throw new Exception(errMessage);
                    }

                    if (!hasContactID(res.EntityData))
                    {
                        errMessage = ediRec.FormatExMsg("The bill to contact_id is null or empty in Customer", EDIMsgType.Internal_Error);

                        throw new Exception(errMessage);
                    }

                    hdr.setTruckProperties(ediRec, res.EntityData);

                    filledOrders.Add(hdr);
                }
                catch (Exception ex)
                {
                    hdr.SetHdrDidNotPass(true);

                    hdr.SetExProps(new EDIException(EDIExceptionType.CustomerTruckServiceException, ExceptionAlertType.rem_only, ex.Message));
                }
            });

            return filledOrders;
        }

        protected bool hasContactID(CustomerTruck res)
        {
            if (string.IsNullOrEmpty(res.contactID))
            {
                return false;
            }

            return true;
        }

        protected bool hasRequiredFields(CustomerTruck res)
        {
            if(string.IsNullOrEmpty(res.shipToId.ToString()) || res.shipToId == 0 
                || string.IsNullOrEmpty(res.customerId.ToString()) || res.customerId == 0
                || string.IsNullOrEmpty(res.preferredLocationID.ToString()) || res.preferredLocationID == 0
                || string.IsNullOrEmpty(res.contactFirstName) 
                || string.IsNullOrEmpty(res.contactLastName)) { 

                return false;
            }

            return true;
        }
    }
}
