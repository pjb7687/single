using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Single2013
{
    public partial class frmUpdater : Form
    {
        public frmUpdater()
        {
            InitializeComponent();
        }

        public static void ThreadProc(object frm)
        {
            Application.Run((Form)frm);
        }

        private void ShowFormAndClose(Form frm)
        {
            Thread t = new Thread(new ParameterizedThreadStart(ThreadProc));
            t.Start(frm);
            Close();
        }

        private void frmUpdater_Load(object sender, EventArgs e)
        {
            WebClient c = new WebClient();
            try
            {
                string changelog = c.DownloadString("https://raw.githubusercontent.com/pjb7687/single/master/CHANGELOG");
                txtChangelog.Text = changelog.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while retrieving update information.");
                ShowFormAndClose(new frmTIRF());
            }
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            ShowFormAndClose(new frmTIRF());
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            Process.Start(fi.DirectoryName + "\\SingleUpdater.exe");
            Process.GetCurrentProcess().Kill();
        }
    }
}
