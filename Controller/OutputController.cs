using OrderEntryEDI.Util;
using REMichel.Lib.Util;
using REMichel.OrderEntryEDI.FileHelpers.Email;
using REMichel.OrderEntryEDI.FileHelpers.OrderWriter;
using REMichel.REMLib.Objects;
using REMichel.WebServicesDomain.DataServiceClients.CodeTable;
using REMichel.WebServicesDomain.DataServiceClients.CodeTable.DTO;
using REMichel.WebServicesDomain.DataServiceClients.EDI;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.Controller
{
    public class ApplicationOutputController
    {
        private EDI     ediRec                  { get; set; }
        private string filePath                 { get; set; }
        private string P21fName                 { get; set; }
        private List<EDIOEHeader> allHeaders    { get; set; }
        private string tempDirectory                    = null;
        private string P21playActiveDir                 = null;
        private List<EDIOEHeader> successOrders         = null;

        public ApplicationOutputController(EDI ediRec, List<EDIOEHeader> allHeaders, string filePath,  string P21fName)
        {
            this.ediRec     = ediRec;
            this.filePath   = ediRec.filePath;
            this.allHeaders = allHeaders;
            this.P21fName   = ediRec.hdrBuildSuccess? ediRec.tradingPartnerName + DateTime.Now.ToString("mmssffffff") + ".txt": null;
        }


        public void EndProcess()
        {
            try
            {
                if (!ediRec.hdrBuildSuccess)
                {
                   InsertAuditRecords(ediRec, allHeaders);
                }
                else
                {
                    successOrders = RetrieveSuccessOrders(allHeaders);

                    if (successOrders.Count > 0)
                    {
                        EDIOEHeader.AssignOELineNo(successOrders);

                        WriteSuccessOrders();

                       //  ImportIntoP21();
                    }
                }

                ediRec.hasFailedHDR = allHeaders.Select(h => h.isHdrDidNotPass()).First();

                InsertAuditRecords(ediRec, allHeaders);
                
                
               if(allHeaders == null || allHeaders.Count == 0)
                {
                    AbstractEmailHandler emailHandler =
                        
                    EmailHandlerFactory.GetInstance(ediRec, filePath, null);

                    if (!emailHandler.Send())
                    {
                        Debug.WriteLine(String.Format("The EDI record for {0} does not contain any email Email Addresses.", ediRec.tradingPartnerName));
                    }
                }

                foreach(EDIOEHeader h in allHeaders)
                {
                    AbstractEmailHandler emailHandler =

                    EmailHandlerFactory.GetInstance(ediRec, filePath, h);

                    if (!emailHandler.Send())
                    {
                        Debug.WriteLine(String.Format("The EDI record for {0} does not contain any email Email Addresses.", ediRec.tradingPartnerName));

                        break;
                    }
                }

                new DirectoryUtil().BuildPath(ediRec, filePath, EDIPathBuilderType.backup);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(String.Format("{0} {1}" ,ex.Source, ex.TargetSite));
                Debug.WriteLine(ex.StackTrace);

                throw new Exception(ex.Message);
            }
        }

        private void WriteSuccessOrders()
        {
            SystemPropertyService sps = new SystemPropertyService();

            List<SystemProperty> sysProps = sps.Get().EntityData.resultSet;

            tempDirectory = sysProps.Single(x => x.sysKey == SystemPropertyKey.EDI_P21_OE_TEMP_DIR.ToString()).value;

            P21playActiveDir = sysProps.Single(x => x.sysKey == SystemPropertyKey.EDI_P21_OE_DIR.ToString()).value;

            new OrderWriter().WriteOrders(successOrders, ediRec, filePath, tempDirectory, P21fName);
        }

        private void ImportIntoP21()
        {
            if(new DirectoryUtil().isNotEmptyDir(tempDirectory))
            {
                foreach(string file in Directory.GetFiles(tempDirectory))
                {
                    File.Copy(file, Path.Combine(P21playActiveDir, Path.GetFileName(file)));
                }
            }
        }

        // BUILDS LIST OF ERROR ORDERS
       /* private void RetrieveOEErrors(List<EDIOEHeader> allHeaders)
        {
            //errLines  = new List<EDIOEHeader>();
            errOrders = new List<EDIOEHeader>();
            EDIOEHeader orderCopy = null;
            try
            {
                foreach (EDIOEHeader h in allHeaders)
                {
                    // total failure
                    if (h.isHdrDidNotPass())
                    {
                        orderCopy = ObjectClone.DeepCopy(h);

                        errOrders.Add(orderCopy);
                    }
                    // failed lines in a successful order
                    if (h.HasRejectedLines())
                    {
                        orderCopy = ObjectClone.DeepCopy(h);
                        orderCopy = RemoveLines(orderCopy, true);

                        errOrders.Add(orderCopy);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }*/

        // BUILDS LIST OF SUCCESS ORDERS
        private List<EDIOEHeader> RetrieveSuccessOrders(List<EDIOEHeader> allHeaders)
        {
            List<EDIOEHeader> successOrders = new List<EDIOEHeader>();

            EDIOEHeader orderCopy = null;
            try
            {
                foreach (EDIOEHeader h in allHeaders)
                {
                    if (!h.isHdrDidNotPass())
                    {
                        h.SetHdrSuccesss(true);

                        orderCopy = new EDIOEHeader();
                        
                        orderCopy = ObjectClone.DeepCopy(h);
                    }
                    // pass with some rejected lines
                    if (!h.isHdrDidNotPass() && h.HasRejectedLines())
                    {
                        RemoveLines(orderCopy, false);

                        successOrders.Add(orderCopy);
                    }
                    // total success
                    else if (!h.isHdrDidNotPass() && !h.HasRejectedLines())
                    {
                        successOrders.Add(orderCopy);
                    }
                }
                return successOrders;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // REMOVES SUCCESS OR FAILES LINES FROM AND ORDER
        private EDIOEHeader RemoveLines(EDIOEHeader order, bool removeSuccessLines)
        {
            foreach(EDIOELine l in order.oeLine.ToArray())
            {
                bool t = l.LineDidNotPass();
                // remove rejected lines
                if (!removeSuccessLines && l.LineDidNotPass())
                {
                    order.oeLine.Remove(l);
                }
                // remove successful lines
                else if (removeSuccessLines && !l.LineDidNotPass())
                {
                    order.oeLine.Remove(l);
                }
            }

            return order;
        }

        private bool InsertAuditRecords(EDI ediRec, List<EDIOEHeader> hdrs)
        {
            if (hdrs == null)
            {
                hdrs = new List<EDIOEHeader>();
            }

            EDIAudit audit = new EDIAudit(ediRec, hdrs);
            
            ServiceResult<bool> results = Task.Run(() =>
            {
                EDIAuditService ediAuditService = new EDIAuditService();

                return ediAuditService.InsertErrRecords(audit);

            }).Result;

            if (!results.isOk())
            {
                string errMessage = ediRec.FormatExMsg("Failed to insert EDI Audit records.", EDIMsgType.Internal_Error);

                throw new EDIException(EDIExceptionType.EDIAuditServiceException, ExceptionAlertType.rem_only, errMessage);
            }

            return true;
        }
    }
}
