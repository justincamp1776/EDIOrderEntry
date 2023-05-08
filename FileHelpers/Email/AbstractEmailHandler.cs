using REMichel.Lib.Util;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.DataServiceClients.Services;
using REMichel.WebServicesDomain.Types;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace REMichel.OrderEntryEDI.FileHelpers.Email
{
    public abstract class AbstractEmailHandler
    {
        protected string filePath                   { get; set; }
        protected string backUpFileLocation         { get; set; }
        protected EDI ediRec                        { get; set; }
        protected EDIOEHeader hdr                   { get; set; }

        protected string errMessage = null;

        public AbstractEmailHandler(EDI ediRec, string filePath, EDIOEHeader hdr)
        {
            this.ediRec             = ediRec;
            this.filePath           = filePath;
            this.hdr                = hdr;
        }

        public virtual bool Send()
        {
            return SendEmail();
        }

        public bool SendEmail()
        {
            try
            {
                var emailBuilder = new EmailBuilder(ediRec, filePath, hdr);

                Lib.Email.EmailMessage emailMsg = emailBuilder.ConfigureEmail();

                if(emailMsg == null)
                {
                    return false;
                }

                ServiceResult<bool> task = Task.Run(async () =>
                {
                    ServicesService sService = new ServicesService();

                    return await sService.SendEmail(emailMsg);

                }).Result;

                if (!task.isOk())
                {
                    Debug.WriteLine(task.GetErrorMsgs());

                    throw new Exception(task.GetErrorMsgs());
                }

                return task.isOk();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
