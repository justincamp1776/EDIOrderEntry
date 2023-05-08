using ClosedXML.Excel;
using REMichel.OrderEntryEDI.Parser;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace OrderEntryEDI.FileHelpers.Parser
{
    /*
        *  'column I, PO creation date (REQUIRED)
           'column A, PO Number (REQUIRED)
           'constant, Company Code
           'column G, Inventory Location (truck number, REQUIRED, only first 10 chars)
           'column N, Vendor Part Number (REM Part, REQUIRED, only first 12 chars)
           'constant, blank space
           'column O, Quantity (REQUIRED AND GREATER THAN 0)
           'column D, Job Number (optional)

           'column K, Material Code(Needed to track skipped lines so that material codes can be listed in email)
        * 
        * */


    public class ExcelParser : IEDIParser
    {
        private EDI ediRec              { get; set; }
        private string filePath         { get; set; }

        private XLWorkbook workbook         = null;

        private IXLWorksheet ws1            = null; 

        private int lastRow                 = 0;

        private string data                 = null;


        List<EDIOERecord> oeRecords = new List<EDIOERecord>();

        public ExcelParser() { }


        public List<EDIOERecord> ReadFile(EDI ediRec, string filePath)
       {
            List<EDIOERecord> oeRecs = new List<EDIOERecord>();

            try
            {
                workbook = new XLWorkbook(filePath);

                ws1 = workbook.Worksheet(1);

                if (!isHas22Columns(ws1))
                {
                    throw new Exception("Header did not contain 22 columns.");
                }

                lastRow = CountRows(ws1);

                oeRecs = BuildOERecords(ws1);

            }
            catch(Exception ex)
            {
                string errMsg = ediRec.FormatExMsg(ex.Message, EDIMsgType.Invalid_Format);

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_only, errMsg);
            }

            return oeRecs;
       }

        private List<EDIOERecord> BuildOERecords(IXLWorksheet ws)
        {
            List<EDIOERecord> oeRecs = new List<EDIOERecord>();
           
            for (int i = 2; i <= lastRow; i++) // (i = 2) = skip hdr
            {
                EDIOERecord rec = new EDIOERecord();

                rec.ItemID = ws.Cell(string.Format("N{0}", i.ToString())).GetValue<string>();//12
                rec.CustomerPartNumber = ws.Cell(string.Format("K{0}", i.ToString())).GetValue<string>();
                rec.OrderDate = ws.Cell(string.Format("I{0}", i.ToString())).GetValue<string>();
                rec.CustomerPONumber = ws.Cell(string.Format("A{0}", i.ToString())).GetValue<string>();
                rec.CompanyNumber = "01";
                rec.Truck = ws.Cell(string.Format("G{0}", i.ToString())).GetValue<string>(); // .Substring(0, 10)
                rec.CustomerPartNumber = "";
                rec.Quantity = Convert.ToDecimal(ws.Cell(string.Format("O{0}", i.ToString())).GetValue<string>());
                rec.JobName = ws.Cell(string.Format("D{0}", i.ToString())).GetValue<string>();

                oeRecs.Add(rec);
                
            }

            return oeRecs;
        }

        private int CountRows(IXLWorksheet ws)
        {
            // i = 2 because hdr = 1
            for(int i = 2; i < 2500; i++)
            {
                data = ws.Cell(string.Format("A{0}", i.ToString())).GetValue<string>();

                if (string.IsNullOrEmpty(data))
                {
                    if(lastRow == 0)
                    {
                        lastRow = i - 1;
                    }
                    
                    if(i == lastRow + 2)
                    {
                        break;
                    }
                }
            }

            return lastRow;
        }

        private bool isHas22Columns(IXLWorksheet ws)
        {
            data = ws.Cell("W1").GetValue<string>();

            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            return true;
        }
    }
}
