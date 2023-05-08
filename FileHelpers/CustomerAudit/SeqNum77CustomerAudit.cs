using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.FileHelpers.CustomerAudit
{
    public class SeqNum77CustomerAudit : AbstractCustomerAudit
    {
        public SeqNum77CustomerAudit(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders, filePath) { }

        public override bool CreateAuditTable()
        {
            try
            {
                if (ediRec.ediCustomerBackupType != EDICustomerBackupType.audit)
                {
                    return true;
                }

                foreach (EDIOEHeader o in orders)
                {
                    foreach (EDIOELine l in o.oeLine)
                    {
                        EDIMichaelSon rec = new EDIMichaelSon();

                        rec.dateSentInFile = Convert.ToInt32(o.OrderDate.Replace("/", ""));
                        rec.poNumber = o.customerPONumber;
                        rec.companyCode = l.GetCompanyCode();
                        rec.accountNumber = Convert.ToInt64(o.customerID);
                        rec.accountCity = o.ShipToCity;
                        rec.branchNumber = o.salesLocationID; ;
                        rec.truckNumber = o.GetTruck();
                        rec.remPartNumber = l.itemID;
                        rec.customerPartNumber = l.GetCustPartNumber();
                        rec.quantityOrdered = l.unitQuantity;
                        rec.fileName = Path.GetFileName(o.GetFilePath());

                        ServiceResult<bool> results = Task.Run(() =>
                        {
                            EDIMichaelSonService ediAuditService = new EDIMichaelSonService();

                            return ediAuditService.InsertMichaelSonsOrder(rec);

                        }).Result;

                        if (!results.isOk())
                        {
                            errMessage = "Failure to insert customer audit records for Michael and Sons.";

                            throw new EDIException(EDIExceptionType.MichaelSonServiceException, ExceptionAlertType.rem_only, errMessage);
                        }
                    }
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
