using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader
{
    public class HDRBuilder
    {
        private string filePath                 { get; set; }
        private EDI ediRec                      { get; set; }
        private List<EDIOERecord> oeRecords     { get; set; }

        private string errMssg                        = null;


        public HDRBuilder(EDI ediRec, List<EDIOERecord> oeRecords, string filePath)
        {
            this.ediRec     = ediRec;
            this.oeRecords  = oeRecords;
            this.filePath   = filePath;
        }

        public List<EDIOEHeader> CreateHeaders()
        {
            List<EDIOEHeader> headers = new List<EDIOEHeader>();
            List<EDIOERecord> filteredOECSVRecords = null;

            try
            {
                IDictionary<string, List<EDIOERecord>> filteredRecords = FilterRecords(oeRecords);

                if (filteredRecords == null || filteredRecords.Count == 0)
                {
                    throw new Exception("FilteredRecords failed to group incoming oeRecords.");
                }

                int count = 1;
                foreach (string key in filteredRecords.Keys)
                {
                    string seconds = DateTime.Now.Second.ToString();
                    string mili = DateTime.Now.Millisecond.ToString();

                    string importSetNo = seconds + mili + count.ToString();

                    count++;

                    bool isRecord = filteredRecords.TryGetValue(key, out filteredOECSVRecords);

                    headers.Add(new EDIOEHeader(importSetNo, filteredOECSVRecords, filePath));
                }

                CheckImportSetNums(headers);

                if (headers != null && headers.Count >= 1)
                {
                    ediRec.hdrBuildSuccess = true;
                }

                return headers;
            }
            catch (Exception ex)
            {
                string msg = ediRec.FormatExMsg(ex.Message, EDIMsgType.Internal_Error);

                throw new EDIException(EDIExceptionType.DuplicateRecordException, ExceptionAlertType.rem_only, msg);
            }
        }

        private  IDictionary<string, List<EDIOERecord>> FilterRecords(List<EDIOERecord> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new Exception("Incoming list of order entry records is null or empty.");
            }

            return
            records.GroupBy(o => (ediRec.seqNum == 1? o.CompanyNumber : o.CustomerPONumber) + o.Truck).ToDictionary(x => x.Key, x => x.ToList());
        }

        private void CheckImportSetNums(List<EDIOEHeader> hdrs)
        {
            List<string> importSetNos = hdrs.Select(h => h.importSetNo).ToList();

            if(importSetNos.Count != importSetNos.Distinct().Count())
            {
                Debug.WriteLine("There are duplicate ImportSetNumbers for this order.");

                throw new Exception("Duplicate ImportSetNumbers were detected for this order.");
            }
        }
    }
}
