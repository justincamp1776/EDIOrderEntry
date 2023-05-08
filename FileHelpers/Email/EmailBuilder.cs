using REMichel.Lib.Email;
using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace REMichel.OrderEntryEDI.FileHelpers.Email
{
    public class EmailBuilder
	{
		private bool fileFailed;
		private bool isCritical;
		private bool hasHeader;
		private bool isDMOSMO_SMO;
		private int totalLines;
		private string companyNo;
		private StringBuilder sb    = null;
		private EDIOEHeader hdr     = null;
		private List<string> TO     = new List<string>();
		private List<string> CC     = new List<string>();

		private EDI ediRec                      { get; set; }
		private string filePath                 { get; set; }
		

		public EmailBuilder(EDI ediRec, string filePath, EDIOEHeader hdr)
		{
			this.ediRec         = ediRec;
			this.filePath       = filePath;
			this.hdr            = hdr;
			hasHeader           = hdr != null ? true : false;
			companyNo           = hdr.GetCompanyCode()??null;
			isDMOSMO_SMO        = ediRec.seqNum == 49 ? true : false;
			totalLines          = hdr != null ? hdr.oeLine.Count:0;
			fileFailed          = !hasHeader ? true : false;
			isCritical          = hdr.isHdrDidNotPass() || !hdr.isHdrDidNotPass() && hdr.HasRejectedLines() || fileFailed? true:false;
		}

		public EmailMessage ConfigureEmail()
		{
            try
            {
                bool alertCustomer = false;

                EmailMessage email = null;

                alertCustomer = hasHeader && String.Equals(hdr.exAlertType, ExceptionAlertType.rem_and_cust.ToString(), StringComparison.OrdinalIgnoreCase)
                                          || hdr.HasRejectedLines() ? true : false;

                if (!RecipientsAreAssigned(isCritical, alertCustomer, ediRec))
                {
                    return null;
                }

                email = new EmailMessage(CreateSubject(isCritical), isCritical);

                email.AddMsgBody(CreateEmailBody());

                email.AddToAddresses(TO);

                email.AddCCAddresses(CC);

                if (!isCritical && ediRec.emailAttachment == EDIEmailAttachment.Y)
                {
                    byte[] byteArr = File.ReadAllBytes(filePath);

                    using (Stream stream = new MemoryStream(byteArr))
                    {
                        email.AddAttachment(new EmailAttachment(filePath, stream, FileType.TXT.ContentType));
                    }
                }

                return email;
            }
			catch(Exception ex)
            {
                throw ex;
            }
		}

	   private bool RecipientsAreAssigned (bool isCritical, bool alertCustomer, EDI ediRec)
		{
			if (alertCustomer)
			{
				TO = ediRec.GetRecipientsAsString(EmailRecipientType.TO, isDMOSMO_SMO, companyNo);

				CC = ediRec.GetRecipientsAsString(EmailRecipientType.CC, isDMOSMO_SMO, companyNo);
			}

			else if (!alertCustomer)
			{
				TO.Add("justin.campbell@remichel.com");
				 
				// set up virtual text through verizon
			}

			if (TO == null || TO.Count == 0)
			{
				return false;
			}

			return true;
		}

		 
		private string CreateSubject(bool isCritical)
		{
			sb = new StringBuilder();

			sb.Append(!ediRec.hdrBuildSuccess && ediRec.exMessage != null ? "R.E.MICHEL EDI: FILE VALIDATION ERROR"
					   : hdr.isHdrDidNotPass() ? "R.E.MICHEL EDI: TERMINAL HEADER ERROR FOR PO#: " + hdr.customerPONumber
					   : hdr.HasRejectedLines()? "R.E.MICHEL EDI: ITEM REJECTION LIST FOR PO#: " + hdr.customerPONumber
					   : "R.E.MICHEL EDI: HEADER PROCESSED WITHOUT ERRORS");

			return sb.ToString();
		}

		private string CreateEmailBody()
		{
			StringBuilder emailBody = new StringBuilder();

			emailBody.Append(isCritical?"*** IMPORTANCE(*HIGH) ***":"");

			emailBody.Append("\n\n");

			emailBody.Append("The R.E.Michel FTP-EDI program has");

			emailBody.Append("<b>");

			emailBody.Append(fileFailed || hdr.isHdrDidNotPass()? " failed to translate a "
							:hdr.HasRejectedLines()? " translated a PO Header with line errors for "
							: " successfully translated a ");

			emailBody.Append(ediRec.name);

			emailBody.Append(!hdr.HasRejectedLines()?" EDI order" : "");

			emailBody.Append(" on \n\n");

			emailBody.Append(DateTime.Now.ToString("F"));

			emailBody.Append("</b>");

			emailBody.Append("\n\n");

			emailBody.Append(fileFailed ? "Our records indicate that " + Path.GetFileName(filePath) + " did not meet validation requirments." : "");

			emailBody.Append(fileFailed && hdr.isHdrDidNotPass() ? "<b>" + " ERROR MESSAGE: " : "");

			emailBody.Append(hdr.isHdrDidNotPass() ? hdr.exMessage 
							: fileFailed ? ediRec.exMessage + "</b>": "");

			emailBody.Append(hdr.HasRejectedLines() ? "PO#" + hdr.customerPONumber+" contained Invalid/ Inactive Items." : "");

			emailBody.Append(hdr.HasRejectedLines() ? "\n\n": "");

			emailBody.Append(hdr.HasRejectedLines() ? "REJECTED ITEMS:" : "");

			emailBody.Append(hdr.HasRejectedLines() ? GetRejectedLines(hdr) : "");

			emailBody.Append(!isCritical ? "" :  "\n\n");

			emailBody.Append(hdr.isHdrDidNotPass() ? "This Header has been deleted. "
							 : fileFailed ? "This File has been moved to your company's error folder."
							 : hdr.HasRejectedLines() ? "These lines have been deleted. If this PO contained additional lines,\nthey have passed validation and are being submitted for processing."
							 : "This order has been submitted for processing." );
							 
			emailBody.Append("\n\n");

			emailBody.Append(!isCritical && ediRec.emailAttachment == EDIEmailAttachment.Y ? "Please see attached." : "");
			

			return emailBody.ToString();
		}

		
		private string GetRejectedLines(EDIOEHeader hdr)
		{
			sb = new StringBuilder();

			sb.Append("\n");

            var errItemsDict = hdr.oeLine.Where(x => x.LineDidNotPass()).ToDictionary(x => x.itemID, x => x.GetLineEx().Message);

            foreach (var i in errItemsDict)
			{
				
				sb.Append(i.Key);
				sb.Append(String.Format(" ({0})", i.Value));
				sb.Append(",");
				sb.Append("\n");
			}

			return sb.ToString();
		}
	}
}
