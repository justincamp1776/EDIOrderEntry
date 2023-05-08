using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.CustomerAudit
{
    public class DefaultCustomerAudit : AbstractCustomerAudit
    {
       public DefaultCustomerAudit(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders, filePath) { }

        public override bool CreateAuditTable()
        {
            return true;
        }
    }
}
