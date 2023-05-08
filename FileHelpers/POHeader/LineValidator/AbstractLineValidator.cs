using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.DataServiceClients.Inventory;
using REMichel.WebServicesDomain.DataServiceClients.Inventory.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REMichel.OrderEntryEDI.FileHelpers.POHeader.LineValidator
{
    public abstract class AbstractLineValidator
    {
        protected EDI ediRec                            { get; set; }
        protected string errMessage                     { get; set; }
        protected List<EDIOEHeader> hdrs                { get; set; }

        protected HashSet<PartQtyHolder> partQtyHolder      = null;

        protected bool isCustomerPartNumber                 = false;

        List<EDIOEHeader> validLines = new List<EDIOEHeader>();


        public AbstractLineValidator(EDI ediRec, List<EDIOEHeader> hdrs)
        {
            this.ediRec     = ediRec;
            this.hdrs       = hdrs;
        }

        public virtual void ValidateLines()
        {
            if(hdrs == null || hdrs.Count == 0)
            {
                ediRec.hdrBuildSuccess = false;

                throw new Exception("A null or empty list of headers was passed into Line Validator.");
            }

            ValidateItems();
        }

        protected void ValidateItems()
        {
            hdrs.ForEach(h =>
            {
                try
                {
                    Dictionary<string, ItemPrice> itemAndPrice = null;

                    if (!h.isHdrDidNotPass())
                    {
                        flagNullItemIDs(h.oeLine);

                        InventoryService invService = new InventoryService();

                        itemAndPrice = invService.getEDIPricing(Convert.ToInt32(h.salesLocationID), Convert.ToInt64(h.customerID), BuildPartQtyHolder(h)).Result.EntityData;
                    }

                   // var rejectItems = itemAndPrice.Where(i => i.Value == null).Select(p => p.Key).ToList();

                    h.oeLine.ForEach(l =>
                    {
                        if (!l.LineDidNotPass())
                        {
                            ItemPrice iP = itemAndPrice.Where(i => i.Key == l.itemID).Select(p => p.Value).FirstOrDefault();

                            // if part does not exist, DS should return a null Item Price obj
                            if(iP == null)
                            {
                                rejectLine(l, "Inactive or Invalid Item_ID.");
                            }
                            else if (!String.Equals(iP.selllable, "Y", StringComparison.InvariantCultureIgnoreCase))
                            {
                                rejectLine(l, "Item Not Sellable at Sales Location.");
                            }

                            // have not identified any other line validation requirements

                            // also remember that each customer defaults to "partial" so qty_on_hand does not matter.
                        }
                    });
                }
                catch(Exception ex)
                {
                    ediRec.abortProcess = true;

                    throw new EDIException(EDIExceptionType.InventoryServiceException, ExceptionAlertType.rem_only, ediRec.FormatExMsg(ex.Message, ex));
                }
            });
        }

        private HashSet<PartQtyHolder> BuildPartQtyHolder(EDIOEHeader hdr)
        {
            partQtyHolder = new HashSet<PartQtyHolder>();

            hdr.oeLine.ForEach(l =>
            {
                if (!l.LineDidNotPass())
                {
                    partQtyHolder.Add(new PartQtyHolder(l));
                }
            });

            return partQtyHolder;
        }

        private void flagNullItemIDs(List<EDIOELine> oeLines)
        {
            oeLines.ForEach(l =>
            {
                if (string.IsNullOrEmpty(l.itemID))
                {
                    l.SetLineDidNotPass(true);

                    errMessage = "Required Field: Item_ID was null or empty. ";

                    l.SetExProps(new EDIException(EDIExceptionType.IsNullOrEmptyException, ExceptionAlertType.rem_and_cust, errMessage)); ;
                }
            });
        }

        private void rejectLine(EDIOELine l, string errMessage)
        {
            l.SetLineDidNotPass(true);

            l.SetExProps(new EDIException(EDIExceptionType.RejectedItemIDException, ExceptionAlertType.rem_and_cust, errMessage));
        }
    }
}
