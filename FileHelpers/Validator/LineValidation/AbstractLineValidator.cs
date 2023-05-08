using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;

namespace REMichel.OrderEntryEDI.FileHelpers.Validator.LineValidation
{
    public abstract class AbstractLineValidator
    {
        protected EDI ediRec                { get; set; }
        protected List<EDIOEHeader> orders  { get; set; }
        protected string filePath           { get; set; }
        protected decimal customerId        { get; set; }

        List<EDIOEHeader> validOrds = new List<EDIOEHeader>();

        public AbstractLineValidator(EDI ediRec, List<EDIOEHeader> orders, string filePath)
        {
            this.ediRec     = ediRec;
            this.orders     = orders;
            this.filePath   = filePath;
        }

        public virtual void ValidateLines()
        {
            try
            {
                orders.ForEach(o => ValidateItemIDs(o));
            }
            catch(Exception e)
            {

            }
        }

        protected void ValidateItemIDs(EDIOEHeader order)
        {
            HashSet<string> itemIDs = null;
            decimal locationID      = 0;
            try
            {
                itemIDs     = BuildHashSet(order);
                locationID  = order.SalesLocationID;
                customerId  = order.CustomerID;

                // Item Service and ServiceClient
            }
            catch (Exception )
            {
                throw;
            }
        }

        private HashSet<string> BuildHashSet(EDIOEHeader order)
        {
            HashSet<string> itemIDs = new HashSet<string>();

            order.OELine.ForEach(l => itemIDs.Add(l.ItemID));

            return itemIDs;
        }
    }
}
