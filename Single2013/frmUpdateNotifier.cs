using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Single2013
{
    public partial class frmUpdateNotifier : Form
    {
        public frmUpdateNotifier()
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

        private void frmUpdateNotifier_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            int major = fvi.FileMajorPart;
            int minor = fvi.FileMinorPart;
            int build = fvi.FileBuildPart;

            WebClient c = new WebClient();
            try
            {
                string versioninfo = c.DownloadString("http://pjb7687.github.io/single/update.txt");
                string[] lines = versioninfo.Split('\n');

                string[] splits = lines[0].Trim('\r').Split('.');
                if (int.Parse(splits[0]) != major || int.Parse(splits[1]) != minor || int.Parse(splits[2]) != build)
                {
                    ShowFormAndClose(new frmUpdater());
                }
                else
                {
                    ShowFormAndClose(new frmTIRF());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while retrieving update information.");
                ShowFormAndClose(new frmTIRF());
            }
        }
    }
}
