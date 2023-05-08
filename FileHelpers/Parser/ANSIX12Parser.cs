using REMichel.OrderEntryEDI.Parser;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace REMichel.OrderEntryEDI.Parser
{
    // TODO: NEED TO WATCH ITEM LINE ELEMENTS. "PO1" IS USED TO ID THE RIGHT LINE IF CHANGED PROGRAM FAILS.
    public class ANSIX12Parser : IEDIParser
    {
        public ANSIX12Parser() { }

        public List<EDIOERecord> ReadFile(EDI ediRec, string filePath)
        {
            List<EDIOERecord> allOrders = new List<EDIOERecord>();
            
            if (!isFile850(filePath))
            {
                throw new Exception("ANSI X12 order entry is not of type 850");
            }

            try
            {
                string str850 = File.ReadAllText(filePath);
                string[] arr850 = str850.Split(new string[] { "BEG" }, StringSplitOptions.None);
                int count = arr850.Length;

                for (int i = 1; i < arr850.Length; i++)
                {
                    string PONumber = arr850[i].Split('*')[3];
                    string OrderDate = GetDate(arr850[i].Split('*')[5].ToCharArray());
                    string Truck = GetTruck(arr850[i]);
                    string[] tempArr = arr850[i].Split(new string[] { "PO1" }, StringSplitOptions.None);


                    for (int j = 1; j < tempArr.Length; j++)
                    {
                        EDIOERecord order = new EDIOERecord();
                        string[] tempArr2 = tempArr[j].Split('*');
                        order.Quantity = long.Parse(tempArr2[2]);
                        order.ItemID = tempArr2[7].Contains("\n") ? tempArr2[7].Split(new string[] { "\n" }, StringSplitOptions.None)[0] : tempArr2[7];
                        order.CustomerPONumber = PONumber;
                        order.OrderDate = OrderDate;
                        order.Truck = Truck;

                        allOrders.Add(order);
                    }
                }

                return allOrders;
            }
            catch (Exception ex)
            {
                string errMsg = ediRec.FormatExMsg(ex.Message, EDIMsgType.Invalid_Format);

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_and_cust, ex.Message);
            }
            finally
            {
                ediRec.orderDate = allOrders != null ? allOrders.Select(r => r.OrderDate).First().Trim() : null;
            }
        }

        private bool isFile850(string filePath)
        {
            string[] arr850 = File.ReadAllLines(filePath);

            if (!arr850[2].Split('*')[1].Contains("850"))
            {
                return false;
            }
            return true;
        }

        private string GetDate(char[] arr)
        {
            var year  = arr[0].ToString() + arr[1].ToString();
            var month = arr[2].ToString() + arr[3].ToString();
            var day   = arr[4].ToString() + arr[5].ToString();

            return month + day + year;
        }

        private string GetTruck(string BEGString)
        {
            string[] tempArr = BEGString.Split(new string[] { "REF" }, StringSplitOptions.None);
            return tempArr[1].Split('*')[2].Replace("\nPO1", "");
        }
    }  
}

