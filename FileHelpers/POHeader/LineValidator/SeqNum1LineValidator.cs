using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.LineValidator
{
    public class SeqNum1LineValidator : AbstractLineValidator
    {
        public SeqNum1LineValidator(EDI ediRec, List<EDIOEHeader> orders, string filePath) : base(ediRec, orders) { }
    }
}
