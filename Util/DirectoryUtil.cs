using REMichel.WebServicesDomain.DataServiceClients.EDI.DTO;
using REMichel.WebServicesDomain.Types;
using System;
using System.IO;
using System.Text;

namespace OrderEntryEDI.Util
{
    public class DirectoryUtil
    {
        // EDIPathTypes = error, backup, or archive 
        public void BuildPath(EDI ediRec, string filePath, EDIPathBuilderType pathType)
        {
            string source = "";

            if (string.IsNullOrEmpty(filePath) || pathType ==  EDIPathBuilderType.archive && ediRec.ediCustomerBackupType != EDICustomerBackupType.archive)
            {
                return;
            }
               
            try
            {
                // checks to see if the file has been moved to backUp directory
                source = ConfirmSourcePath(ediRec, filePath);

                string uniqueKey = DateTime.Now.ToString("yyyyMMddHHmmss");

                StringBuilder sb = new StringBuilder();

                sb.Append(ediRec.serverName);

                // 1ST AND 2ND APPEND BUILD ROOT. FILE LOCATION DIFFERES BY TYPE
                if (pathType != EDIPathBuilderType.archive) //!archive
                {
                    sb.Append(ediRec.fileLocationBackup850);

                    if (pathType == EDIPathBuilderType.backup) //backup
                    {
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }

                        sb.Append(Path.GetFileName(source));

                        File.Move(source, sb.ToString());
                    }

                    // GEN APPEND
                    else if (pathType == EDIPathBuilderType.error) //error
                    {
                        sb.Append("ErrorFiles\\");

                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                    }
                } 
                // GEN APPEND
                else if (pathType == EDIPathBuilderType.archive && ediRec.ediCustomerBackupType == EDICustomerBackupType.archive)
                {
                    sb.Append(ediRec.fileLocation850);
                    sb.Append("POArchives\\");

                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                }
                
                // BUILDING UNIQUE PATH
                if (pathType != EDIPathBuilderType.backup)
                {
                    sb.Append(Path.GetFileNameWithoutExtension(source));
                    sb.Append("_");
                    sb.Append(uniqueKey);
                    sb.Append(Path.GetExtension(source));
                   
                    if(pathType == EDIPathBuilderType.error)
                    {
                        File.Move(source, sb.ToString());
                    }   
                    else if (pathType == EDIPathBuilderType.archive && ediRec.ediCustomerBackupType == EDICustomerBackupType.archive)
                    {
                        File.Copy(source, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                string errMessage = ex.Message;

                throw new EDIException(EDIExceptionType.InvalidFormatException, ExceptionAlertType.rem_only, errMessage);
            }
        }

        private string ConfirmSourcePath(EDI ediRec, string filePath)
        {
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(ediRec.serverName, ediRec.fileLocationBackup850.TrimStart('\\'), Path.GetFileName(filePath));
            }

            return filePath;
        }

        public bool isNotEmptyDir(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("File path is null.");
            }

            var folder = new DirectoryInfo(filePath);
            if (folder.Exists)
            {
                if (folder.GetFileSystemInfos().Length == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
