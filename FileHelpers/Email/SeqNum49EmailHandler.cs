using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.DataServiceClients.Services;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.FileHelpers.Email
{
    public class SeqNum49EmailHandler : AbstractEmailHandler
    {
        List<List<EDIOEHeader>> DMOSMO_Company_Orders = null;


        public SeqNum49EmailHandler(EDI ediRec, string fileName, EDIOEHeader hdr) 
            : base(ediRec, fileName, hdr) { }
    

        public override bool Send()
        {
            FilterRecords();

            DMOSMO_Company_Orders = CheckOrderList(DMOSMO_Company_Orders);

            var emailBuilder = new EmailBuilder(ediRec, filePath, hdr);

            Lib.Email.EmailMessage emailMsg = emailBuilder.ConfigureEmail();

            ServiceResult<bool> task = Task.Run(async () =>
            {
                ServicesService sService = new ServicesService();

                return await sService.SendEmail(emailMsg);

            }).Result;

            if (!task.isOk())
            {
                Debug.WriteLine(task.GetErrorMsgs());

                throw new EDIException(EDIExceptionType.ServiceServiceException, ExceptionAlertType.rem_only, task.GetErrorMsgs());
            }

            return true;
        }

        public void FilterRecords()
        {
            try
            {
                List<EDIOEHeader> allHeaders = null;


                List<EDIOEHeader> DMOSMO15_Orders = new List<EDIOEHeader>();
                List<EDIOEHeader> DMOSMO16_Orders = new List<EDIOEHeader>();
                List<EDIOEHeader> DMOSMO17_Orders = new List<EDIOEHeader>();
                List<EDIOEHeader> Misc_Orders = new List<EDIOEHeader>();


                string[] _smoCompanyNums = { "15", "16", "17" };

                foreach (string companyNumber in _smoCompanyNums)
                {
                    foreach (EDIOEHeader h in allHeaders)
                    {
                        switch (companyNumber)
                        {
                            case "15":
                                DMOSMO15_Orders.Add(h);
                                break;
                            case "16":
                                DMOSMO16_Orders.Add(h);
                                break;
                            case "17":
                                DMOSMO17_Orders.Add(h);
                                break;
                            default:
                                Misc_Orders.Add(h);
                                break;
                        }
                    }
                }

                DMOSMO_Company_Orders = new List<List<EDIOEHeader>>();

                DMOSMO_Company_Orders.Add(DMOSMO15_Orders);
                DMOSMO_Company_Orders.Add(DMOSMO16_Orders);
                DMOSMO_Company_Orders.Add(DMOSMO17_Orders);

            }
            catch(Exception ex)
            {
                string errMessage = "Failed to seperate DMOSMO orders by companyNumber";

                throw new EDIException(EDIExceptionType.ServiceServiceException, ExceptionAlertType.rem_only, ediRec.FormatExMsg(errMessage, ex));
            }   
        }

        private List<List<EDIOEHeader>> CheckOrderList(List<List<EDIOEHeader>> allCompanies)
        {
            foreach(List<EDIOEHeader> singleCompany in allCompanies)
            {
                if(singleCompany == null || singleCompany.Count == 0)
                {
                    allCompanies.Remove(singleCompany);
                }
            }

            return allCompanies;
        }
    }
}
