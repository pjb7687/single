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
using System.Threading;
using Microsoft.Win32;

namespace SingleUpdater
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void UpdateDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = (int)(e.ProgressPercentage / 100.0 * 50 + 25);
        }

        private void tmrTimer_Tick(object sender, EventArgs e)
        {
            bool isKilled = true;
            foreach (Process proc in Process.GetProcesses()) {
               if (proc.ProcessName == "Single2013" || proc.ProcessName.StartsWith("Single "))
                    isKilled = false;
            }
            if (isKilled)
            {
                tmrTimer.Enabled = false;
                progressBar1.Value = 0;
                lblStatus.Text = "Checking Update Info...";
                lblStatus.Invalidate();
                lblStatus.Update();
                lblStatus.Refresh();
                Application.DoEvents();
                WebClient c = new WebClient();
                c.DownloadProgressChanged += new DownloadProgressChangedEventHandler(UpdateDownloadProgress);
                try
                {
                    string versioninfo = c.DownloadString("http://pjb7687.github.io/single/update.txt");
                    string url = versioninfo.Split('\n')[1];

                    progressBar1.Value = 25;
                    lblStatus.Text = "Downloading file...";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();
                    c.DownloadFileAsync(new Uri(url), "update.zip");
                    while (progressBar1.Value < 75) {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    progressBar1.Value = 75;
                    lblStatus.Text = "Decompressing downloaded file...";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();
                    FileInfo fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
                    System.IO.File.Move(Application.ExecutablePath, Application.ExecutablePath + ".old");
                    using (ZipArchive archive = ZipFile.OpenRead("update.zip"))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine(fi.DirectoryName, entry.FullName), true);
                        }
                    }

                    progressBar1.Value = 100;
                    lblStatus.Text = "Done!";
                    lblStatus.Invalidate();
                    lblStatus.Update();
                    lblStatus.Refresh();
                    Application.DoEvents();

                    System.IO.File.Delete("update.zip");
                    if (MessageBox.Show(null, "Done! Do you want to run Single?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        Process.Start(fi.DirectoryName + "\\Single2013.exe");

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Application.ExecutablePath + ".old\"";
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    Process delProc = new Process();
                    delProc.StartInfo = startInfo;
                    delProc.Start();

                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MessageBox.Show("An error occured while updating. Please download and install Single manually!");
                    Application.Exit();
                }
            }
        }
    }
}
