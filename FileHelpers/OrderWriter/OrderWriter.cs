using CsvHelper;
using CsvHelper.Configuration;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.OrderWriter
{
    //TODO delete tempTargetDirs
    public class OrderWriter
    {
        private string errMessage = null;
        

        private static CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HasHeaderRecord = false,
        };

        public void WriteOrders(List<EDIOEHeader> orders, EDI ediRec, string sourceFilePath, string tempDirectoryP21, string fName)
        {
            if (orders == null || orders.Count == 0)
            {
                errMessage = "Arg List<EDIOEHeader> is null or empty.";

                throw new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_only, errMessage);
            }


            bool orderHeaderWriteSuccess = false;

            string orderHeaderFilepath = Path.Combine(tempDirectoryP21, "SOH_" + fName);
       
            try
            {
                orderHeaderWriteSuccess = WriteHeader(orders, orderHeaderFilepath);

                List<EDIOELine> lineList = orders.SelectMany(x => x.oeLine).Cast<EDIOELine>().ToList();
                lineList.Sort((x, y) => x.ImportSetNumber.CompareTo(y.ImportSetNumber));

                WriteLines(lineList, tempDirectoryP21, sourceFilePath, ediRec, fName);

            }
            catch (Exception ex)
            {
                // Remove order header file if written to disk and an exception occurred after the fact.
                if (orderHeaderWriteSuccess)
                {
                    try
                    {
                        File.Delete(orderHeaderFilepath);
                    } 
                    catch (Exception e)
                    {
                        // The order header file should be orphaned if an exception occurs. Only move name of orders that were successfully 
                        // (both header/lines) written to disk.
                        Debug.WriteLine(e.StackTrace);
                    }
                }

                throw new EDIException(EDIExceptionType.OrderWriterException, ExceptionAlertType.rem_only, ediRec.FormatExMsg(ex.Message, ex));

            }
        }

        private bool WriteHeader(List<EDIOEHeader> orders, string targetDir)
        {
            using (StreamWriter _hdrStream = new StreamWriter(targetDir))
            using (var csv = new CsvWriter(_hdrStream, config))
            {
                csv.WriteRecords(orders);
            }

            return true;
        }

        private void WriteLines(List<EDIOELine> lines, string tempDirectoryP21, string sourceFilePath, EDI ediRec, string fName)
        {
            string targetDir = Path.Combine(tempDirectoryP21, "SOL_" + fName);
            using (StreamWriter _hdrStream = new StreamWriter(targetDir))
            using (var csv = new CsvWriter(_hdrStream, config))
            {
                csv.WriteRecords(lines);
            }
        }
    }
}