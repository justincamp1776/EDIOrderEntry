using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;

namespace REMichel.OrderEntryEDI.FileHelpers.Email
{
    public class DefaultEmailHandler : AbstractEmailHandler
    {
        public DefaultEmailHandler(EDI ediRec,  string fileName, EDIOEHeader order)
            : base(ediRec, fileName, order) { }
    }
}
    