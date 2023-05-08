using CsvHelper;
using CsvHelper.Configuration;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace REMichel.OrderEntryEDI.Parser
{
    public class CSVParser : IEDIParser
    {
        public CsvConfiguration config = null;
        public CSVParser()
        {
            config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false, };
        }
        
        public List<EDIOERecord> ReadFile(EDI ediRec, string filePath)
        {
            List<EDIOERecord> records = null;
            string errMessage = null;

            try
            {
                using (var streamReader = new StreamReader(File.OpenRead(@filePath)))
                using (var csv = new CsvReader(streamReader, config))
                {
                    switch (ediRec.ediFormat)
                    {
                        case EDIFormat.CSV7:
                            csv.Context.RegisterClassMap<OEMainMap7>();
                            records = csv.GetRecords<EDIOERecord>().ToList();
                            return records;

                        case EDIFormat.CSV6:
                            csv.Context.RegisterClassMap<OEMainMap6>();
                            records = csv.GetRecords<EDIOERecord>().ToList();
                            return records;

                        case EDIFormat.CSV4:
                            csv.Context.RegisterClassMap<OEMainMap4>();
                            records = csv.GetRecords<EDIOERecord>().ToList();
                            return records;

                        default:

                            throw new Exception("Encountered invalid EDI format in parser.");
                            
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = ediRec.FormatExMsg(ex.Message, EDIMsgType.Invalid_Format);
                throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_only, errMsg);
            }
            finally
            {
                ediRec.orderDate = records != null?records.Select(r => r.OrderDate).First().Trim():null;
            }
        }

        //CSV has 6 commas 
        public class OEMainMap6 : ClassMap<EDIOERecord>
        {
            public OEMainMap6()
            {
                Map(m => m.OrderDate).Index(0);
                Map(m => m.CustomerPONumber).Index(1);
                Map(m => m.CompanyNumber).Index(2);
                Map(m => m.Truck).Index(3);
                Map(m => m.ItemID).Index(4);
                Map(m => m.CustomerPartNumber).Index(5);
                Map(m => m.Quantity).Index(6);
            }
        }

        //CSV has 7 commmas
        public class OEMainMap7 : OEMainMap6
        {
            public OEMainMap7() : base() 
            {
                Map(m => m.JobName).Index(7);
            }
        }

        //CSV has 4 commmas
        public class OEMainMap4 : ClassMap<EDIOERecord>
        {
            public OEMainMap4() 
            {
                Map(m => m.OrderDate).Index(0);
                Map(m => m.CompanyNumber).Index(1);
                Map(m => m.Truck).Index(2);
                Map(m => m.ItemID).Index(3);
                Map(m => m.Quantity).Index(4);
            }
        }
    }
}