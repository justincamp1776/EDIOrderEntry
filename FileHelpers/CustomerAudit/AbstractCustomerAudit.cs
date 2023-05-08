using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.CustomerAudit
{
    public abstract class AbstractCustomerAudit
    {
        protected EDI ediRec                { get; set; }
        protected List<EDIOEHeader> orders  { get; set; }
        protected string filePath           { get; set; }

        protected string errMessage        = null;

        public AbstractCustomerAudit(EDI ediRec, List<EDIOEHeader> orders, string filePath)
        {
            this.ediRec = ediRec;
            this.orders = orders;
            this.filePath = filePath;
        }

        public abstract bool CreateAuditTable();
    }
}
