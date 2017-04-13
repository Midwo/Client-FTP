using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlientFTP
{
    class Ftp
    {
        private string host;
        private string userName;
        private string password;
        private string ftpDirectory;
        private bool downloadCompleted;
        private bool uploadCompleted;

        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }
        public string FtpDirectory
        {
            get
            {
                if (ftpDirectory.StartsWith("ftp://"))
                    return ftpDirectory;
                else
                    return "ftp://" + ftpDirectory;
            }
            set
            {
                ftpDirectory = value;
            }
        }
        public bool DownloadCompleted
        {
            get
            {
                return downloadCompleted;
            }
            set
            {
                downloadCompleted = value;
            }
        }
        public bool UploadCompleted
        {
            get
            {
                return uploadCompleted;
            }
            set
            {
                uploadCompleted = value;
            }
        }
    }
}
