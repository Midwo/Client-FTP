using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KlientFTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
                       
        }
        private Ftp client = new Ftp();

        private void buttonLocation_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBoxLocation.Text = folderBrowserDialog1.SelectedPath;
        }


        private void GetFtpContent(ArrayList directoriesList)
        {
            listBoxFtp.Items.Clear();
            listBoxFtp.Items.Add("[...]");
            directoriesList.Sort();
            foreach (string name in directoriesList)
            {
                string position = name.Substring(name.LastIndexOf(' ') + 1,
                name.Length - name.LastIndexOf(' ') - 1);
                if (position != ".." && position != ".")
                    switch (name[0])
                    {
                        case 'd':
                            listBoxFtp.Items.Add("[" + position + "]");
                            break;
                        case 'l':
                            listBoxFtp.Items.Add("->" + position);
                            break;
                        default:
                            listBoxFtp.Items.Add(position);
                            break;
                    }
            }
        }
        void client_DownCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                MessageBox.Show("Błąd: " + e.Error.Message);
            else
                MessageBox.Show("Plik pobrany");
            client.DownloadCompleted = true;
            buttonDownload.Enabled = true;
            buttonSend.Enabled = true;
        }

        void client_DownProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Pobrano: " + (e.BytesReceived / (double)1024).ToString() + " kB";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (comboBoxServer.Text != string.Empty & comboBoxServer.Text.Trim() != String.Empty)
                try
                {
                    string serverName = comboBoxServer.Text;
                    if (serverName.StartsWith("ftp://"))
                        serverName = serverName.Replace("ftp://", "");
                    client = new Ftp(serverName, textBoxLogin.Text, maskedTextBoxPassword.Text);
                    client.DownProgressChanged += new Ftp.DownProgressChangedEventHandler(client_DownProgressChanged);
                    client.DownCompleted += new Ftp.DownCompletedEventHandler(client_DownCompleted);
                    client.UpCompleted += new Ftp.UpCompletedEventHandler(client_UpCompleted);
                    client.UpProgressChanged += new Ftp.UpProgressChangedEventHandler(client_UpProgressChanged);

                    GetFtpContent(client.GetDirectories());
                    textBoxFtp.Text = client.FtpDirectory;
                    toolStripStatusLabel1.Text = "Serwer: ftp://" + client.Host;
                    buttonConnect.Enabled = false;
                    buttonDisconnect.Enabled = true;
                    buttonDownload.Enabled = true;
                    buttonSend.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
            {
                MessageBox.Show("Wprowadź nazwę serwera FTP", "Błąd");
                comboBoxServer.Text = string.Empty;
            }
        }

        private void listBoxFtp_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBoxFtp.SelectedIndex;
            try
            {

                if (index > -1)
                {
                    if (index == 0)
                        GetFtpContent(client.ChangeDirectoryUp());

                    else
                    if (listBoxFtp.Items[index].ToString()[0] == '[')
                    {
                        string directory = listBoxFtp.Items[index].ToString().Substring(1, listBoxFtp.Items[index].ToString().Length - 2);
                        GetFtpContent(client.ChangeDirectory(directory));
                    }
                    else
                        if (listBoxFtp.Items[index].ToString()[0] == '-' & listBoxFtp.Items[index].ToString()[2] == '.')
                    {
                        string link =
                        listBoxFtp.Items[index].ToString().Substring(5,
                        listBoxFtp.Items[index].ToString().Length - 5);
                        client.FtpDirectory = "ftp://" + client.Host;
                        GetFtpContent(client.ChangeDirectory(link));
                    }
                    else
                        this.buttonUpFolder_Click(sender, e);
                    listBoxFtp.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }
            textBoxFtp.Text = client.FtpDirectory;

        }

        private void buttonUpFolder_Click(object sender, EventArgs e)
        {
            client.ChangeDirectoryUp();
            textBoxFtp.Text = client.FtpDirectory;
            GetFtpContent(client.GetDirectories());
        }

        private void listBoxFtp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.listBoxFtp_MouseDoubleClick(sender, null);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            int index = listBoxFtp.SelectedIndex;
            if (listBoxFtp.Items[index].ToString()[0] != '[')
            {
                if (MessageBox.Show("Czy pobrać plik?", "Pobieranie pliku",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    try
                    {
                        string localFile = textBoxLocation.Text + "\\" +
                        listBoxFtp.Items[index].ToString();
                        FileInfo fi = new FileInfo(localFile);
                        if (fi.Exists == false)
                        {
                            client.DownloadFileAsync(listBoxFtp.Items[index].ToString(), localFile);
                            buttonDownload.Enabled = false;
                            buttonSend.Enabled = false;
                        }
                        else
                            MessageBox.Show("Plik istnieje ");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Błąd");
                    }
                }
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    client.UploadFileAsync(openFileDialog1.FileName);
                    buttonDownload.Enabled = false;
                    buttonSend.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd");
                }
            }
        }

        void client_UpCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show("Błąd: " + e.Error.Message);
                client.UploadCompleted = true;
                buttonSend.Enabled = true;
                buttonDownload.Enabled = true;
                return;
            }
            client.UploadCompleted = true;
            buttonSend.Enabled = true;
            buttonDownload.Enabled = true;
            MessageBox.Show("Wysłano plik");
            try
            {
                GetFtpContent(client.GetDirectories());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }
        }
        void client_UpProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Wysłano: " + (e.BytesSent /
            (double)1024).ToString() + " kB";
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                int indeks = listBoxFtp.SelectedIndex;
                if (indeks > -1)
                    if (listBoxFtp.Items[indeks].ToString()[0] != '[')
                    {
                        try
                        {
                            MessageBox.Show(client.DeleteFile(listBoxFtp.Items[indeks].ToString()));
                            GetFtpContent(client.GetDirectories());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Nie można usunąć pliku" + " (" + ex.Message + ")");
                        }
                    }
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            client.DownProgressChanged -= new Ftp.DownProgressChangedEventHandler(client_DownProgressChanged);
            client.DownCompleted -= new Ftp.DownCompletedEventHandler(client_DownCompleted);
            client.UpCompleted -= new Ftp.UpCompletedEventHandler(client_UpCompleted);
            client.UpProgressChanged -= new Ftp.UpProgressChangedEventHandler(client_UpProgressChanged);
            buttonConnect.Enabled = true;
            listBoxFtp.Items.Clear();
            textBoxFtp.Text = "";
        }

    
        private void toolStripComboBox1_Click_1(object sender, EventArgs e)
        {
            string queryString = "SELECT FTP, Login, Password FROM dbo.AccountsFTP Where Name = '"+ toolStripComboBox1.Text.Trim() +"';";
            using (SqlConnection conn1 = new SqlConnection(Properties.Settings.Default.connect))
            {
                SqlDataAdapter da1 = new SqlDataAdapter();
                SqlCommand cmd = conn1.CreateCommand();
                cmd.CommandText = queryString;
                da1.SelectCommand = cmd;
                DataSet ds1 = new DataSet();

                conn1.Open();
                da1.Fill(ds1);
                conn1.Close();

                try
                {
                    comboBoxServer.Text = ds1.Tables[0].Rows[0][0].ToString();
                    textBoxLogin.Text = ds1.Tables[0].Rows[0][1].ToString();
                    maskedTextBoxPassword.Text = ds1.Tables[0].Rows[0][2].ToString();
                }
                catch
                {

                }
                
                //comboBox1.DataSource = ds.Tables[0];
                // comboBox1.ValueMember = "Name";
                // comboBox1.ValueMember = "Name";
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            string queryString = "SELECT * FROM dbo.AccountsFTP;";
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connect))
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = queryString;
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();

                conn.Open();
                da.Fill(ds);
                conn.Close();




                toolStripComboBox1.ComboBox.BindingContext = this.BindingContext;
                toolStripComboBox1.ComboBox.DataSource = ds.Tables[0];
                toolStripComboBox1.ComboBox.DisplayMember = "Name";

                //comboBox1.DataSource = ds.Tables[0];
                // comboBox1.ValueMember = "Name";
                // comboBox1.ValueMember = "Name";
            }
            }
        }
}


