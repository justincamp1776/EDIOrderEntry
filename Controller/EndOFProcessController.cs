using REMichel.OrderEntryEDI.FileHelpers.Email;
using OrderEntryEDI.Util;
using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.Controller
{
    public class EndOFProcessController
    {
        private EDI    ediRec                   { get; set; }
        private string filePath                 { get; set; }
        private string orderDate                { get; set; }
        private string P21fName                 { get; set; }

        private string errMessage              = null;

        private List<EDIException> exceps       = new List<EDIException>();
        private List<EDIOEHeader> filledOrders  = new List<EDIOEHeader>();
        private List<EDIOrderAudit> orderAudits = new List<EDIOrderAudit>();

        public EndOFProcessController(EDI ediRec, List<EDIOEHeader> orders, string filePath, string orderDate, string P21fName)
        {
            this.ediRec             = ediRec;
            this.filePath           = filePath;
            this.orderDate          = orderDate  != null? orderDate.Trim():null;
            this.P21fName           = P21fName;

            filledOrders            = EDIFunctions.PopulateList(orders, filledOrders);
            exceps                  = EDIFunctions.PopulateList(ediRec.GetExceps(), exceps);
        }


        // TODO replace ExceptionType Ref to EDIException

        public void EndProcess(string p21PlayImportDir, string tempDirectory)
        {
            try
            {
                if (!hasRequiredParams(filledOrders, exceps))
                {
                    return;
                }

                EDIFileAudit fileAudit = new EDIFileAudit();

                if (!EDIFunctions.isEmptyList(exceps))
                {
                    foreach (EDIException ex in exceps)
                    {
                        if(ex.failedOrder == null)
                        {
                            Debug.WriteLine(ex.TargetSite);
                        }

                        AssignFileAuditFields(fileAudit, ediRec, filePath, ex);

                        EDIOrderAudit ordAud = AssignOrderAudit(ediRec, ex.failedOrder, filePath, null, ex);

                        fileAudit.AddOrderAudit(ordAud);

                        new DirectoryUtil().BuildPath(ediRec, filePath, EDIPathBuilderType.error);

                        EDIOEHeader order = ex.failedOrder ?? null;
                       
                        /*if (fileAudit == null)
                        {
                            ExceptionType.IsNullOrEmpty = new ExceptionType

                            ("Failed to create exception audit records.", "IsNullOrEmpty AuditRecords", ediRec, order.GetFilePath(), null, typeof(EndOFProcessController), ExceptionAlertType.rem_only, order);

                            throw ExceptionType.IsNullOrEmpty;
                        }*/


                        AbstractEmailHandler emailHandler =

                        EmailHandlerFactory.GetInstance(ediRec, ex, filePath, null, false, null);

                        emailHandler.Send();
                    }

                    ediRec.ClearExceptions();
                }

                if (!EDIFunctions.isEmptyList(filledOrders))
                {
                    fileAudit = AssignFileAuditFields(fileAudit, ediRec, filePath, null);

                    foreach (EDIOEHeader order in filledOrders)
                    {
                        EDIOrderAudit oAudit = AssignOrderAudit(ediRec, order, filePath, P21fName, null);

                        fileAudit.AddOrderAudit(oAudit);

                        if (!EDIFunctions.isEmptyList(fileAudit.GetOrderAudits()))
                        {
                            errMessage = "Failed to create order audit records.";

                            throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_only, null, errMessage, null);
                        }
                    }

                    AbstractEmailHandler emailHandler =

                    EmailHandlerFactory.GetInstance(ediRec, null, filePath, null, true, filledOrders);

                    emailHandler.Send();

                    filledOrders.Clear();
                }

                ServiceResult<bool> results = Task.Run(() =>
                {
                    EDIAuditService ediAuditService = new EDIAuditService();

                        return ediAuditService.InsertEDIAudit(fileAudit);

                }).Result;

                if (!results.isOk())
                {
                    errMessage = "Failed to insert EDI Audit records.";

                    throw new EDIException(EDIExceptionType.EDIAuditServiceException, ExceptionAlertType.rem_only, null, errMessage, null);
                }
                


                //BACKUP METHOD COMMENTED OUT FOR TESTING
                //new DirectoryUtil().BuildPath(ediRec, filePath, EDIPathBuilderType.backup);


                /*if(new DirectoryUtil().isNotEmptyDir(tempDirectory))
                {
                    foreach(string file in Directory.GetFiles(tempDirectory))
                    {
                        File.Copy(file, Path.Combine(p21PlayImportDir, Path.GetFileName(file)));
                    }
                }*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                throw ex;
            }
        }

        private EDIOrderAudit AssignOrderAudit(EDI rec, EDIOEHeader o, string filePath, string P21fName, EDIException ex)
        {
            EDIOrderAudit ordAud = new EDIOrderAudit();

            if(o != null)
            {
                ordAud.customerPO   = o.CustomerPONumber;
                ordAud.truck        = o.GetTruck();
                ordAud.branchID     = o.SalesLocationID;
                ordAud.customerID   = o.CustomerID;
                ordAud.shipToID     = o.ShipToID;
                ordAud.importSetNo  = o.importSetNo;
            }
          
            return ordAud;
        }

        private EDIFileAudit AssignFileAuditFields(EDIFileAudit fileAudit, EDI ediRec, string filePath, EDIException ex)
        {
            bool isSuccess   = ex != null ? false : true;
            bool hasInner   = !isSuccess && ex.InnerException != null ? true : false;

            string exMessage = !isSuccess && ex.InnerException != null && ex.InnerException.Message != null ? ex.InnerException.Message
                            : !isSuccess ? ex.Message : null;


            fileAudit.orderDate             = orderDate;
            fileAudit.incomingFileName      = filePath;
            fileAudit.ediSeqNum             = ediRec.seqNum;
            fileAudit.importedFlag          = false;
            fileAudit.successFlag           = isSuccess ? true : false;
            fileAudit.p21ImportFileName     = isSuccess? P21fName:null;

            fileAudit.exType                = !isSuccess ? ex.EDIExceptionType.ToString() : null;
            fileAudit.exMessage             = !isSuccess && hasInner && ex.InnerException.Message != null ? ex.InnerException.Message
                                              : !isSuccess ? ex.Message : null;

            fileAudit.exLocation            = !isSuccess && hasInner && ex.InnerException.TargetSite != null? ex.InnerException.TargetSite.ToString()
                                            : !isSuccess && ex.TargetSite != null? ex.TargetSite.ToString():null; // TODO add source to target site

            return fileAudit;
        }

        // Some customers send files that REM doesn't service. Emails should not be sent regarding these files
        private bool hasRequiredParams(List<EDIOEHeader> orders, List<EDIException> exeps)
        {
            if(EDIFunctions.isEmptyList(exeps) && EDIFunctions.isEmptyList(orders))
            {
                return false;
            }

            return true;
        }
    }
}
