using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.Parser
{
    public interface IEDIParser
    {
       List<EDIOERecord> ReadFile (EDI ediRec, string filePath);
    }
}