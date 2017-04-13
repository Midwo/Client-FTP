using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public Ftp()
        {
            downloadCompleted = true;
            uploadCompleted = true;
        }

        public Ftp(string host, string userName, string password)
        {
            this.host = host;
            this.userName = userName;
            this.password = password;
            ftpDirectory = "ftp://" + this.host;
        }

        public ArrayList GetDirectories()
        {
            ArrayList directories = new ArrayList();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)WebRequest.Create(ftpDirectory);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(this.userName,
                this.password);
                request.KeepAlive = false;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string directory;
                        while ((directory = reader.ReadLine()) != null)
                        {
                            directories.Add(directory);
                        }
                    }
                }
                return directories;
            }
            catch
            {
                throw new Exception("Błąd: Nie można nawiązać połączenia z " + host);
            }
        }

        public ArrayList ChangeDirectory(string DirectoryName)
        {
            ftpDirectory += "/" + DirectoryName;
            return GetDirectories();
        }

        public ArrayList ChangeDirectoryUp()
        {
            if (ftpDirectory != "ftp://" + host)
            {
                ftpDirectory = ftpDirectory.Remove(ftpDirectory.LastIndexOf("/"), ftpDirectory.Length - ftpDirectory.LastIndexOf("/"));
                return GetDirectories();
            }
            else
                return GetDirectories();
        }

        public void DownloadFileAsync(string ftpFileName, string localFileName)
        {
            WebClient client = new WebClient();
            try
            {
                Uri uri = new Uri(ftpDirectory + "/" + ftpFileName);
                FileInfo file = new FileInfo(localFileName);
                if (file.Exists)
                    throw new Exception("Błąd: Plik " + localFileName + " istnieje");
                else
                {
                    client.DownloadFileCompleted += new System.ComponentModel.
                AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.Credentials = new NetworkCredential(this.userName, this.password);
                    client.DownloadFileAsync(uri, localFileName);
                    downloadCompleted = false;
                }
            }
            catch
            {
                client.Dispose();
                throw new Exception("Błąd: Pobranie pliku niemożliwe");
            }
        }

        public delegate void DownProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);

        public event DownProgressChangedEventHandler DownProgressChanged;

        protected virtual void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownProgressChanged != null) DownProgressChanged(sender, e);
        }

        public delegate void DownCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

        public event DownCompletedEventHandler DownCompleted;

        protected virtual void OnDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownCompleted != null) DownCompleted(sender, e);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.OnDownloadProgressChanged(sender, e);
        }
        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.OnDownloadCompleted(sender, e);
        }

        public void UploadFileAsync(string FileName)
        {
            try
            {
                System.Net.Cache.RequestCachePolicy cache = new System.Net.Cache.
                RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Reload);
                WebClient client = new WebClient();
                FileInfo file = new FileInfo(FileName);
                Uri uri = new Uri((FtpDirectory + '/' + file.Name).ToString());
                client.Credentials = new NetworkCredential(this.userName, this.password);
                uploadCompleted = false;
                if (file.Exists)
                {
                    client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
                    client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged);
                    client.UploadFileAsync(uri, FileName);
                }
            }
            catch
            {
                throw new Exception("Błąd: Nie można wysłać pliku");
            }
        }

        void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            this.OnUploadProgressChanged(sender, e);
        }
        void client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            this.OnUploadCompleted(sender, e);
        }

        public delegate void UpCompletedEventHandler(object sender, UploadFileCompletedEventArgs e);
        public event UpCompletedEventHandler UpCompleted;
        protected virtual void OnUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if (UpCompleted != null) UpCompleted(sender, e);
        }

        public delegate void UpProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);
        public event UpProgressChangedEventHandler UpProgressChanged;
        protected virtual void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (UpProgressChanged != null) UpProgressChanged(sender, e);
        }

        public string DeleteFile(string nazwa)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpDirectory + "//" + nazwa);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(this.userName,
                this.password);
                request.KeepAlive = false;
                request.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                return response.StatusDescription;
            }
            catch (Exception ex)
            {
                throw new Exception("Błąd: Nie można usunąć pliku " + nazwa +" (" + ex.Message + ")");
            }
        }
    }
}