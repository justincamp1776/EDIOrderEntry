
using OrderEntryEDI.FileHelpers.Parser;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;

namespace REMichel.OrderEntryEDI.Parser
{
    public class ParserFactory
    {
        public static IEDIParser GetInstance(EDI ediRec, string filePath)
        {
            switch (ediRec.ediFormat)
            {
                case EDIFormat.CSV4:
                case EDIFormat.CSV6:
                case EDIFormat.CSV7:
                    return new CSVParser();

                case EDIFormat.X12:
                    return new ANSIX12Parser();

                case EDIFormat.EXCEL:
                    return new ExcelParser();

                default:
                    string errMessage = "Invalid EDI format detected in Parser Factory.";

                    throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_only, errMessage);
            }
        }
    }
}
