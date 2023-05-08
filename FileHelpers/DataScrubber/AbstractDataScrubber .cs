using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Globalization;


namespace REMichel.OrderEntryEDI.FileHelpers.DataScrubber
{
    public abstract class AbstractDataScrubber
    {
        // TODO uncomment out CheckDates() for Prod.

        protected EDI ediRec                        { get; set; }
        protected string filePath                   { get; set; }
        protected List<EDIOERecord> oeRecords       { get; set; }

        protected string errMessage = null;

        protected AbstractDataScrubber(EDI ediRec, List<EDIOERecord> oeRecords, string filePath)
        {
            this.oeRecords      = oeRecords;
            this.filePath       = filePath;
            this.ediRec         = ediRec;
        }

        public List<EDIOERecord> scrubData()
        {
            try
            {
                if (!InitScrubData() || !ScrubData())
                {
                    throw new Exception(errMessage);
                }

                return oeRecords;
            }
            catch (Exception ex)
            {
                string errMsg = errMessage == null ? ex.Message : ediRec.FormatExMsg(errMessage, EDIMsgType.Invalid_Format);

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_and_cust, errMsg);
            }
            
        } 

        protected abstract bool ScrubData();

       
        protected bool ReAssignQTYtoFirstIndex()
        {
            try
            {
                oeRecords.ForEach(o => o.Quantity = o.Quantity.ToString().Contains(".") ? long.Parse(o.Quantity.ToString().Split('.')[0]) : o.Quantity);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = "Failed to remove decimal and reassign from quantity. Ex.Message: " + ex.Message;

                return false;
            }
        }

        protected bool ReAssignQTYtoOne()
        {
            try
            {
                oeRecords.ForEach(o => o.Quantity = o.Quantity.ToString().Contains(".") ? 1 : o.Quantity);

                return true;
            }
            catch(Exception ex)
            {
                errMessage = "Failed to remove decimal and reassign from quantity. Ex.Message: " + ex.Message;

                return false;
            }
        }

        

        private bool InitScrubData()
        {
            try
            {
                //!CheckDates()
                if (!CheckDateFormat())
                {
                    return false;
                } 
               
                oeRecords.ForEach(o => o.CustomerPONumber = o.CustomerPONumber?.Trim());
                oeRecords.ForEach(o => o.CompanyNumber = o.CompanyNumber?.Trim());
                oeRecords.ForEach(o => o.Truck = o.Truck?.Trim());
                oeRecords.ForEach(o => o.ItemID = o.ItemID?.Trim());
                oeRecords.ForEach(o => o.CustomerPartNumber = o.CustomerPartNumber?.Trim());
                oeRecords.ForEach(o => o.Quantity = decimal.Parse(o.Quantity.ToString()?.Trim()));
                oeRecords.ForEach(o => o.JobName = o.JobName?.Trim());

                oeRecords.ForEach(o => o.Truck  = o.Truck.Replace("~", ""));
                oeRecords.ForEach(o => o.ItemID = o.ItemID.Replace("~", ""));
                oeRecords.ForEach(r => r.ItemID = r.ItemID.Length > 12 ? r.ItemID.Substring(0, 12) : r.ItemID);
                oeRecords.ForEach(r => r.Truck = r.Truck.Length > 12 ? r.Truck.Substring(0, 12) : r.Truck);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;

                return false;
            }
        }

        private string AddSlashesToDate(string date)
        {
            DateTime dt;
            int length = date.Length;
           
            switch (length)
            {
                case 6:
                    dt = DateTime.ParseExact(date, "MMddyy", CultureInfo.InvariantCulture);
                    return dt.ToString("MM/dd/yyyy");

                case 8:
                    dt = DateTime.ParseExact(date, "MM/dd/yy", CultureInfo.InvariantCulture);
                    return dt.ToString("MM/dd/yyyy");

                case 7:
                    dt = DateTime.ParseExact(date, "MM/d/yy", CultureInfo.InvariantCulture);
                    return dt.ToString("MM/dd/yyyy");

                case 9:
                    dt = DateTime.ParseExact(date, "MM/d/yyyy", CultureInfo.InvariantCulture);
                    return dt.ToString("MM/dd/yyyy");

                default:
                    throw new Exception("Invalid Date Format.");
            }
        }

        private bool CheckDateFormat()
        {
            try
            {
                oeRecords.ForEach(o => o.OrderDate = o.OrderDate?.Trim());
                oeRecords.ForEach(o => o.OrderDate =   (!o.OrderDate.Contains("/") && o.OrderDate.Length == 8)
                                                    || (!o.OrderDate.Contains("/") && o.OrderDate.Length == 6) ? AddSlashesToDate(o.OrderDate)
                                                    :    o.OrderDate);
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ediRec.FormatExMsg("Encountered invalid date format.", ex);

                return false;
            }
        }

        private bool CheckDates()
        {
            try
            {
                string today = DateTime.Now.ToString("MM/dd/yy");
                oeRecords.ForEach(o => o.OrderDate = o.OrderDate != DateTime.Now.ToString("MM/dd/yy") ?
                                                    throw new Exception("Detected an order date that does not match today's date.")
                                                    : o.OrderDate);
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;

                return false;
            }
        }
    }
}