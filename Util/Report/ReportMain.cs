using REMichel.WebServicesDomain.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;

namespace OrderEntryEDI.Util.Report
{
    // main
    public class ReportMain
    {
        private string fileName     { get; set; }
        private string sourcePath   { get; set; }

        private string hdrFailCountStr  = null;

        private string lineFailCountStr = null;

        private Timer aTimer            = null;


        public ReportMain(string fileName)
        {
            this.fileName = Path.GetFileNameWithoutExtension(fileName);
        }


        public void CheckImportResults()
        {
            if (!hasErrorReports())
            {
                Debug.WriteLine("EDI Logger confirms success of fileName:" + fileName);
            }

            // GenerateErrorReport
        }

        public bool hasErrorReports()
        {
            string sourcePath = ReportUtil.BuildPath(fileName, EDIPathBuilderType.summary, ".sum");

            string [] sumArray = File.ReadAllLines(sourcePath);

            foreach(string ln in sumArray)
            {
                AssignTargetStrings(ln);
            }

            if (string.IsNullOrEmpty(hdrFailCountStr) || string.IsNullOrEmpty(lineFailCountStr))
            {
                throw new Exception("Failed to find failed record count lines in Summary for EDI Log");
            }

           return isSuccess(hdrFailCountStr, lineFailCountStr);
        }

        private void AssignTargetStrings(string ln)
        {
            bool has1 = ln.ToLower().Contains(ReportUtil.GetEnumDescription(EDILogKeyWords.hdr).ToLower());
            bool has2 = ln.ToLower().Contains(ReportUtil.GetEnumDescription(EDILogKeyWords.ln).ToLower());
            bool has3 = ln.ToLower().Contains(ReportUtil.GetEnumDescription(EDILogKeyWords.updt).ToLower());
            bool has4 = ln.ToLower().Contains(ReportUtil.GetEnumDescription(EDILogKeyWords.fail).ToLower());

            if(has1 && has3 && has4)
            {
                hdrFailCountStr = ln;
            }

            if (has2 && has3 && has4)
            {
                lineFailCountStr = ln;
            }
        }

        private bool isSuccess(string line1, string line2)
        {
            string[] arr1 = line1.Split(' ').ToArray();
            string[] arr2 = line2.Split(' ').ToArray();

            if(Convert.ToInt32(arr1[arr1.Length - 1]) > 0 || Convert.ToInt32(arr2[arr2.Length - 1]) > 0)
            {
                return false;
            }

            return true;
        }
    }
}
