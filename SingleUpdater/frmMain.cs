using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace SingleUpdater
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private string getDownloadFolderPath()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
        }

        private void tmrTimer_Tick(object sender, EventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("Single2013");
            if (pname.Length == 0)
            {
                tmrTimer.Enabled = false;
                progressBar1.Value = 25;
                lblStatus.Text = "Checking Update Info...";
                lblStatus.Invalidate();
                lblStatus.Update();
                lblStatus.Refresh();
                Application.DoEvents();
                WebClient c = new WebClient();
                try
                {
                    string versioninfo = c.DownloadString("http://pjb7687.github.io/single/update.txt");
                    string url = versioninfo.Split('\n')[1];

                    progressBar1.Value = 50;
                    lblStatus.Text = "Downloading file...";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();
                    c.DownloadFile(url, "update.zip");

                    progressBar1.Value = 75;
                    lblStatus.Text = "Decompressing downloaded file...";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();
                    FileInfo fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                    using (ZipArchive archive = ZipFile.OpenRead("update.zip"))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (!entry.FullName.EndsWith("SingleUpdater.exe", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(fi.DirectoryName, entry.FullName), true);
                            }
                        }
                    }

                    progressBar1.Value = 100;
                    lblStatus.Text = "Done!";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();

                    if (MessageBox.Show(null, "Done! Do you want to run Single?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        Process.Start(fi.DirectoryName + "\\Single2013.exe");
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MessageBox.Show("An error occured while updating. Please download and install Single manually!");
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}
