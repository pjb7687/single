using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
using System.IO;

using SMBdevices;
using System.Windows;
using System.IO.Ports;

namespace Single2013
{
    public partial class frmTIRF : Form
    {
        private smbCCD m_ccd;
        private smbShutter m_shutter;
        private smbStage m_stage;
        private int m_selectedpump = -1;

        private ImageDrawer m_imgdrawer;
        private AutoFocusing m_autofocusing;
        private AutoFlow m_autoflow;
        private ActiveDriftCorrection m_adc;

        public int[] cursorXY = new int[2] {-1, -1};

        public bool m_CCDon = false;

        private string m_pmafiledir;
        private string m_pmafilenamehead;

        public int m_chanum;

        public frmTIRF()
        {
            InitializeComponent();
        }

        #region Delegates For MultiThreading
        public delegate void updateAutoScaleInfoDelegate(int theMax, int[] theMin);
        public void updateAutoScaleInfo(int theMax, int[] theMin)
        {
            double scaler = (double)(theMax - theMin.Min()) / 256;
            NumericUpDown tmp;

            if (scaler < 1) scaler = 1;
            TextBoxScaler.Text = scaler.ToString();

            for (int i = 0; i < Math.Min(SplitConSubs.Panel2.Controls.Count, theMin.Length); i++)
            {
                tmp = (NumericUpDown)SplitConSubs.Panel2.Controls[i];
                tmp.Value = theMin[i];
            }
        }

        public delegate void updateFilmingInfoDelegate(int framenum);
        public void updateFilmingInfo(int framenum)
        {
            LabelFramenum.Text = "Frame #: " + framenum.ToString();
            TimeSpan t = TimeSpan.FromSeconds( framenum * Convert.ToDouble(TextBoxExptime.Text) );
            string time = string.Format("{0:D2}h {1:D2}m {2:D2}s {3:D3}ms", 
    		                            	t.Hours, 
    			                            t.Minutes, 
    			                            t.Seconds, 
    			                            t.Milliseconds);
            LabelTime.Text = "Time: " + time;
        }

        public delegate void updateAFInfoDelegate(double fom, double dist);
        public void updateAFInfo(double fom, double dist)
        {
            ChartAFDistance.Series[0].Points.AddY(dist);
            ChartAFFOM.Series[0].Points.AddY(fom);
            ChartAFDistance.ChartAreas[0].AxisY.Minimum = ChartAFDistance.Series[0].Points.FindMinByValue().YValues[0] - 1;
            ChartAFDistance.ChartAreas[0].AxisY.Maximum = ChartAFDistance.Series[0].Points.FindMaxByValue().YValues[0] + 1;
            ChartAFFOM.ChartAreas[0].AxisY.Minimum = ChartAFFOM.Series[0].Points.FindMinByValue().YValues[0] - 0.1;
            ChartAFFOM.ChartAreas[0].AxisY.Maximum = ChartAFFOM.Series[0].Points.FindMaxByValue().YValues[0] + 0.1;
            if (ChartAFDistance.Series[0].Points.Count > 500) ChartAFDistance.Series[0].Points.RemoveAt(0);
            if (ChartAFFOM.Series[0].Points.Count > 500) ChartAFFOM.Series[0].Points.RemoveAt(0);
            LabelAFInfo.Text = "FOM: " + fom.ToString("\\0.#####E0") + "\nDIST: " + dist.ToString("\\0.#####E0");
        }

        public delegate void AutoStopDelegate();
        public void AutoStop()
        {
            StartFilmingButton_Click(null, new EventArgs());
        }

        public delegate void FindingFocalPointDoneDelegate();
        public delegate void CalculateSTDEVDoneDelegate(double[] foms, double stdev);
        public delegate void CalibrateDoneDelegate(double[] dists, double[] foms, double[] fitvals);

        public void CalibrateDone(double[] dists, double[] foms, double[] fitvals)
        {
            int i;
            Log("[Auto Focusing]", new string[] { "Slope: " + fitvals[1].ToString() });
            Log("[Auto Focusing]", new string[] { "Roughly finding the best focal point..." });

            ChartAFDistance.Series.Clear();
            ChartAFDistance.Series.Add("Dists");
            ChartAFDistance.Series.Add("Linear Fit");
            ChartAFDistance.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            ChartAFDistance.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            for (i = 0; i < dists.Length; i++)
                ChartAFDistance.Series[0].Points.AddXY(foms[i], dists[i]);
            ChartAFDistance.Series[1].Points.AddXY(foms[0], fitvals[0] + fitvals[1] * foms[0]);
            ChartAFDistance.Series[1].Points.AddXY(foms[foms.Length - 1], fitvals[0] + fitvals[1] * foms[foms.Length - 1]);
            ChartAFDistance.ChartAreas[0].AxisY.Minimum = ChartAFDistance.Series[0].Points.FindMinByValue().YValues[0] - 0.01;
            ChartAFDistance.ChartAreas[0].AxisY.Maximum = ChartAFDistance.Series[0].Points.FindMaxByValue().YValues[0] + 0.01;
            TextBoxAFSlope.Text = fitvals[1].ToString();
            m_autofocusing.FindingFocalPoint();
        }

        public void FindingFocalPointDone()
        {
            Log("[Auto Focusing]", new string[] { "Calculating STDEV..."});
            m_autofocusing.CalculateSTDEV();
        }

        public void CalculateSTDEVDone(double[] foms, double stdev)
        {
            int i, j, cnt;
            double binstart, binend;
            double binstep = (foms.Max() - foms.Min()) / 10;
            Log("[Auto Focusing]", new string[] { "STDEV: " + stdev.ToString() });
            ChartAFFOM.Series.Clear();
            ChartAFFOM.Series.Add("Freq. Count of FOM");
            ChartAFFOM.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            binstart = foms.Min(); binend = binstart + binstep;
            for (i = 0; i < 10; i++)
            {
                cnt = 0;
                for (j = 0; j < foms.Length; j++)
                    if ((foms[j] >= binstart) && (foms[j] < binend)) cnt++;
                binstart += binstep; binend += binstep;
                ChartAFFOM.Series[0].Points.AddY(cnt);
            }
            ChartAFFOM.ChartAreas[0].AxisY.Minimum = 0;
            ChartAFFOM.ChartAreas[0].AxisY.Maximum = ChartAFFOM.Series[0].Points.FindMaxByValue().YValues[0];
            TextBoxAFStdev.Text = stdev.ToString();
            ButtonAFCalibration.Enabled = true;
            ButtonAFStart.Enabled = true;
        }

        public delegate void updateAFLInfoDelegate(int index, double volume);
        public void updateAFLInfo(int index, double volume)
        {
            ListViewAFLRules.ItemCheck -= new ItemCheckEventHandler(ListViewAFLRules_ItemCheck);
            ListViewAFLRules.Items[index].Checked = true;
            ListViewAFLRules.ItemCheck += new ItemCheckEventHandler(ListViewAFLRules_ItemCheck);
            Log("[Auto Flow]", new string[] {"Flowed volume: " + volume.ToString()});
        }

        #endregion

        #region Helper Methods

        private void LoadAllSettings()
        {
            // Settings Related
            ComboBoxBinSize.SelectedIndex = Properties.Settings.Default.BinSizeIndex;
            ComboBoxZoomMode.SelectedIndex = Properties.Settings.Default.ZoomModeIndex;
            ComboBoxCCDModel.SelectedIndex = Properties.Settings.Default.CCDModelIndex;
            NUDImagingWidth.Value = Properties.Settings.Default.ImagingWidth;
            NUDImagingHeight.Value = Properties.Settings.Default.ImagingHeight;
            NUDCameraIndex.Value = Properties.Settings.Default.CameraIndex;

            if (m_ccd != null) m_ccd.Dispose();
            m_ccd = new smbCCD((smbCCD.CCDType)Properties.Settings.Default.CCDModelIndex, Properties.Settings.Default.CameraIndex);
            m_ccd.SetBinSize(Convert.ToInt32(ComboBoxBinSize.Items[Properties.Settings.Default.BinSizeIndex]));
            m_ccd.SetRange((int)NUDImagingWidth.Value, (int)NUDImagingHeight.Value);
            m_ccd.SetTemp(-85);

            switch (ComboBoxZoomMode.SelectedIndex)
            {
                case 0:
                    CCDWindow.SizeMode = PictureBoxSizeMode.CenterImage;
                    break;
                case 1:
                    CCDWindow.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
                case 2:
                    CCDWindow.SizeMode = PictureBoxSizeMode.Zoom;
                    break;
            }

            TextBoxPMAHead.Text = Properties.Settings.Default.PMAhead;
            TextBoxPMAPath.Text = Properties.Settings.Default.PMASavePath;

            ListViewLasers.Items.Clear();
            for (int i = 0; i < Properties.Settings.Default.Lasers.Count; i += 2)
                ListViewLasers.Items.Add(new ListViewItem(new string[] { Properties.Settings.Default.Lasers[i], Properties.Settings.Default.Lasers[i + 1] }));

            ListViewCounters.Items.Clear();
            for (int i = 0; i < Properties.Settings.Default.Counters.Count; i += 4)
                ListViewCounters.Items.Add(new ListViewItem(new string[] {Properties.Settings.Default.Counters[i],
                                                                          Properties.Settings.Default.Counters[i + 1],
                                                                          Properties.Settings.Default.Counters[i + 2],
                                                                          Properties.Settings.Default.Counters[i + 3]}));

            LaserCheckedListBox.Items.Clear();
            try
            {
                for (int i = 0; i < Properties.Settings.Default.Lasers.Count; i += 2)
                    AddLaserToListBox(Properties.Settings.Default.Lasers[i], Properties.Settings.Default.Lasers[i + 1]);

                ALEXCheckedListBox.Items.Clear();
                for (int i = 0; i < Properties.Settings.Default.Counters.Count; i += 4)
                    AddCounterBoardToListBox(Properties.Settings.Default.Counters[i],
                                             Properties.Settings.Default.Counters[i + 1],
                                             Properties.Settings.Default.Counters[i + 2],
                                             Properties.Settings.Default.Counters[i + 3]);
            }
            catch { }

            // Drawer Class
            m_imgdrawer = null;
            m_imgdrawer = new ImageDrawer(Properties.Settings.Default.Colortable, this, m_ccd);

            // PMA file path setting
            m_pmafiledir = Properties.Settings.Default.PMASavePath;
            m_pmafilenamehead = Properties.Settings.Default.PMAhead;
        }

        private void AddLaserToListBox(string lasername, string lines)
        {
            m_shutter.AddLaser(lines);
            LaserCheckedListBox.Items.Add(lasername);
        }

        private void AddCounterBoardToListBox(string lasername, string counter, string countersrc, string triggersrc)
        {
            m_shutter.AddCounterBoard(counter, countersrc, triggersrc);
            ALEXCheckedListBox.Items.Add(lasername);
        }

        private void OffAllLaser()
        {
            for (int i = 0; i < LaserCheckedListBox.Items.Count; i++)
                m_shutter.LaserOff(i);
        }

        private void Log(string mainissue, string[] issues)
        {
            string crlf = "\r\n";
            LogTextBox.Text += crlf + crlf + mainissue;
            for (int i=0; i<issues.Length; i++)
                LogTextBox.Text += crlf + issues[i];
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            LogTextBox.ScrollToCaret();
        }

        private void Log(string mainissue)
        {
            string crlf = "\r\n";
            LogTextBox.Text += crlf + crlf + mainissue;
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            LogTextBox.ScrollToCaret();
        }
        #endregion

        #region Control Events
        private void frmTIRF_Load(object sender, EventArgs e)
        {
            LogTextBox.Text = "Single 2013 - TIRF by Jeongbin Park\r\n\r\nThis program has been inspired by the original 'Single' program by SH and WS.";

            // Shutters
            m_shutter = new smbShutter(smbShutter.ShutterType.NI_DAQ);

            LoadAllSettings();

            OffAllLaser();

            NUDChannelNum.Value = 2;
            SetGainButton_Click(sender, e);

            LogTextBox.Text += "\r\nReady.";
            Log("[Camera]", new string[] { "Temperature of CCD is set to " + Convert.ToString(-85) + " C." });
        }

        private void frmTIRF_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (m_ccd.GetTemp() < -20)
            {
                MessageBox.Show("In order to shutdown whole system, Temperature of CCD should be above -20 C.", "Single 2013");
                e.Cancel = true;
            }*/
            if (m_autofocusing != null)
                m_autofocusing.m_focusing = false;
            if (m_autoflow != null)
                m_autoflow.m_autoflow = false;
            m_imgdrawer.m_auto = false;
            m_imgdrawer.m_filming = false;
            m_imgdrawer.StopDrawing();
            OffAllLaser();
        }

        private void OpenCameraButton_Click(object sender, EventArgs e)
        {
            if (!m_CCDon)
            {
                OpenCameraButton.Text = "Close Camera";
                m_ccd.ShutterOn();
                m_imgdrawer.m_auto = CheckBoxAuto.Checked;
                m_imgdrawer.StartDrawing(CCDWindow, m_ccd);
                Log("[Camera]", new string[] { "Shutter Opened.", "Bin Size: " + m_ccd.m_binsize.ToString(), "Stretch Mode: " + CCDWindow.SizeMode.ToString() });
            }
            else
            {
                if (m_imgdrawer.m_filming) StartFilmingButton_Click(sender, e);
                m_imgdrawer.StopDrawing();
                m_ccd.ShutterOff();
                OpenCameraButton.Text = "Open Camera";
                Log("[Camera]", new string[] {"Shutter Closed."});
            }
            StartFilmingButton.Enabled = !m_CCDon;
            SetTempButton.Enabled = m_CCDon;
            GetTempButton.Enabled = m_CCDon;
            GroupBoxCCDSettings.Enabled = m_CCDon;
            GroupBoxDAQSettings.Enabled = m_CCDon;
            GroupBoxFilmingSettings.Enabled = m_CCDon;
            SetGainButton.Enabled = m_CCDon;
            NUDChannelNum.Enabled = m_CCDon;
            ButtonAFConnect.Enabled = m_CCDon;
            ButtonAFLFindDevices.Enabled = m_CCDon;
            ListViewAFLPumps.Enabled = m_CCDon;
            m_CCDon = !m_CCDon;
        }

        private void GetTempButton_Click(object sender, EventArgs e)
        {
            TempLabel.Text = m_ccd.GetTemp().ToString();
        }

        private void NUDChannelNum_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown sub;
            if (NUDChannelNum.Value < 1) NUDChannelNum.Value = 1;
            if (NUDChannelNum.Value > 8) NUDChannelNum.Value = 8;
            while (NUDChannelNum.Value > SplitConSubs.Panel2.Controls.Count)
            {
                sub = new NumericUpDown();
                SplitConSubs.Panel2.Controls.Add(sub);
                sub.Dock = DockStyle.Bottom;
                sub.Maximum = 100000;
                sub.TextAlign = HorizontalAlignment.Center;
                sub.ValueChanged += new EventHandler(NUDsub_TextChanged);
            }
            for (int i = SplitConSubs.Panel2.Controls.Count-1; i >= (int)NUDChannelNum.Value; i--)
            {
                SplitConSubs.Panel2.Controls.RemoveAt(i);
            }
            GainGroupBox.Height = 190+SplitConSubs.Panel2.Controls[0].Height * (int)NUDChannelNum.Value + SplitConSubs.Panel2.Padding.Bottom + SplitConSubs.Panel2.Padding.Top;
            NUDChannelNum.BackColor = System.Drawing.Color.LightPink;
        }

        private void SetTempButton_Click(object sender, EventArgs e)
        {
            m_ccd.SetTemp((int)NUDTemp.Value);
        }

        private void CheckBoxAuto_CheckedChanged(object sender, EventArgs e)
        {
            m_imgdrawer.m_auto = CheckBoxAuto.Checked;
        }
        private void SetGainButton_Click(object sender, EventArgs e)
        {
            double scaler, exptime;
            
            if (!double.TryParse(TextBoxScaler.Text, out scaler)) {scaler = 1; TextBoxScaler.Text = "1";}
            if (!double.TryParse(TextBoxExptime.Text, out exptime)) {exptime = 0.1; TextBoxExptime.Text = "0.1";}

            int[] subs = new int[SplitConSubs.Panel2.Controls.Count];
            NumericUpDown tmp;
            for (int i = 0; i < SplitConSubs.Panel2.Controls.Count; i++)
            {
                tmp = (NumericUpDown)SplitConSubs.Panel2.Controls[i];
                subs[i] = (int)tmp.Value;
            }
            m_imgdrawer.SetValues(scaler, subs);
            m_ccd.SetExpTime(exptime);
            m_ccd.SetGain((int)NUDGain.Value);

            NUDGain.BackColor = System.Drawing.SystemColors.Window;
            TextBoxScaler.BackColor = System.Drawing.SystemColors.Window;
            TextBoxExptime.BackColor = System.Drawing.SystemColors.Window;
            NUDChannelNum.BackColor = System.Drawing.SystemColors.Window;
            foreach (NumericUpDown sub in SplitConSubs.Panel2.Controls)
            {
                sub.BackColor = System.Drawing.SystemColors.Window;
            }
            m_chanum = (int)NUDChannelNum.Value;
            NUDAFRange.Maximum = m_chanum;

            Log("[Camera]", new string[] { "Exposure time: " + m_ccd.GetExptime().ToString() });

        }

        private void StartFilmingButton_Click(object sender, EventArgs e)
        {
            if (m_imgdrawer.m_filming) {
                m_imgdrawer.StopFilming();
                StartFilmingButton.Text = "Start Filming";
                m_shutter.StopALEX();
                if (m_autofocusing != null)
                    m_autofocusing.m_ignoredarkframe = 0;
                LaserCheckedListBox.Enabled = true;
                for (int i = 0; i < LaserCheckedListBox.Items.Count; i++)
                {
                    if (LaserCheckedListBox.GetItemChecked(i)) m_shutter.LaserOn(i);
                    else m_shutter.LaserOff(i);
                }
                if (m_autoflow != null)
                    if (m_autoflow.m_autoflow)
                        ButtonAFLEnable_Click(sender, e);
                Log("[Filming]", new string[] {"Filming Stopped."});
            }
            else
            {
                m_imgdrawer.m_framenum = 0;
                int filecnt = 1;
                string filename = "";
                string logfilename = "";

                do
                {
                    filename = m_pmafiledir + "\\" + m_pmafilenamehead + filecnt.ToString() + ".pma";
                    logfilename = m_pmafiledir + "\\" + m_pmafilenamehead + filecnt.ToString() + ".log";
                    filecnt++;
                } while (File.Exists(filename));

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(logfilename))
                {
                    file.WriteLine("Exposure time:  " + TextBoxExptime.Text);
                    file.WriteLine("Acquisition mode:  Full " + m_ccd.m_imagewidth.ToString() + "x" + m_ccd.m_imageheight.ToString() + " " + m_ccd.m_binsize.ToString() + "x" + m_ccd.m_binsize.ToString() + " Binning");
                    file.WriteLine("Gain:  " + NUDGain.Value.ToString());
                    file.WriteLine("Data scaler:  " + TextBoxScaler.Text);
                    string str = "Background subtraction:";
                    foreach (NumericUpDown sub in SplitConSubs.Panel2.Controls)
                        str += "  " + sub.Value.ToString();
                    file.WriteLine(str);
                }

                Log("[Filming]", new string [] { "Filming Started.", "File Name: " + filename });
                StartFilmingButton.Text = "Stop Filming";
                using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var bw = new BinaryWriter(fileStream))
                {
                    bw.Write(BitConverter.GetBytes((short)m_ccd.m_imagewidth));
                    bw.Write(BitConverter.GetBytes((short)m_ccd.m_imageheight));
                }

                m_imgdrawer.StartFilming(filename);
                
                if (ALEXCheckedListBox.CheckedItems.Count > 1)
                {
                    LaserCheckedListBox.Enabled = false;
                    if (m_autofocusing != null && CheckBoxAFIgnoreDarkFrame.Checked)
                        m_autofocusing.m_ignoredarkframe = ALEXCheckedListBox.CheckedItems.Count;
                    m_shutter.StartALEX(m_ccd.m_exptime);
                }
            }
        }

        private void LaserCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ALEXCheckedListBox.CheckedItems.Count > 1 && m_imgdrawer.m_filming) return;
            if (e.NewValue == CheckState.Checked)
                m_shutter.LaserOn(e.Index);
            else
                m_shutter.LaserOff(e.Index);
        }

        private void ALEXCheckedListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (ALEXCheckedListBox.CheckedItems.Count > 1)
            {
                m_shutter.ClearALEX();
                foreach (int index in ALEXCheckedListBox.CheckedIndices)
                    m_shutter.AddALEX(index);
            }
        }

        private void NUDGain_ValueChanged(object sender, EventArgs e)
        {
            NUDGain.BackColor = System.Drawing.Color.LightPink;
        }

        private void TextBoxScaler_TextChanged(object sender, EventArgs e)
        {
            if (!m_imgdrawer.m_auto)
                TextBoxScaler.BackColor = System.Drawing.Color.LightPink;
        }

        private void TextBoxExptime_TextChanged(object sender, EventArgs e)
        {
            TextBoxExptime.BackColor = System.Drawing.Color.LightPink;
        }

        private void NUDsub_TextChanged(object sender, EventArgs e)
        {
            if (!m_imgdrawer.m_auto)
            {
                NumericUpDown sub = (NumericUpDown)sender;
                sub.BackColor = System.Drawing.Color.LightPink;
            }
        }

        private void CheckBoxAutoStop_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAutoStop.Checked)
                m_imgdrawer.m_autostopframenum = (int)NUDStopFrame.Value;
            else
                m_imgdrawer.m_autostopframenum = 0;
        }

        private void CheckBoxShowGuidelines_CheckedChanged(object sender, EventArgs e)
        {
            m_imgdrawer.ToggleGuidelines(CheckBoxShowGuidelines.Checked);
        }

        private void CCDWindow_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                int[] xy = new int[2] {((MouseEventArgs)e).Y, ((MouseEventArgs)e).X};
                if (xy[0] > 5 && xy[0] < 507 && xy[1] > 5 && xy[1] < 507)
                    cursorXY = xy;
            }
            else
            {
                cursorXY = new int[2] { -1, -1 };
            }
        }
        #endregion

        #region 'Device Settings' Tab Related
        private void ButtonAddLaser_Click(object sender, EventArgs e)
        {
            AddModify frm = new AddModify();
            frm.Text = "Add Laser...";
            frm.static1 = "Name: ";
            frm.static2 = "Port/Channel: ";
            if (frm.ShowDialog() == DialogResult.OK)
                ListViewLasers.Items.Add(new ListViewItem(new string[] { frm.text1, frm.text2 }));
        }

        private void ButtonAddCounter_Click(object sender, EventArgs e)
        {
            AddModify2 frm = new AddModify2();
            frm.Text = "Add CounterBoard...";
            frm.static1 = "Name: ";
            frm.static2 = "Port/Channel: ";
            frm.static3 = "Timebase: ";
            frm.static4 = "Trigger: ";
            if (frm.ShowDialog() != DialogResult.OK) return;
            
            ListViewLasers.Items.Add(new ListViewItem(new string[] { frm.text1, frm.text2, frm.text3, frm.text4 }));
        }

        private void ButtonModifyLaser_Click(object sender, EventArgs e)
        {
            if (ListViewLasers.SelectedItems.Count != 1) return;

            AddModify frm = new AddModify();
            frm.Text = "Modify Laser...";
            frm.static1 = "Name: ";
            frm.static2 = "Port/Channel: ";
            frm.text1 = ListViewLasers.SelectedItems[0].SubItems[0].Text;
            frm.text2 = ListViewLasers.SelectedItems[0].SubItems[1].Text;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ListViewLasers.SelectedItems[0].SubItems[0].Text = frm.text1;
                ListViewLasers.SelectedItems[0].SubItems[1].Text = frm.text2;
            }
        }

        private void ButtonModifyCounter_Click(object sender, EventArgs e)
        {
            if (ListViewCounters.SelectedItems.Count != 1) return;

            AddModify2 frm = new AddModify2();
            frm.Text = "Add CounterBoard...";
            frm.static1 = "Name: ";
            frm.static2 = "Port/Channel: ";
            frm.static3 = "Timebase: ";
            frm.static4 = "Trigger: ";
            frm.text1 = ListViewCounters.SelectedItems[0].SubItems[0].Text;
            frm.text2 = ListViewCounters.SelectedItems[0].SubItems[1].Text;
            frm.text3 = ListViewCounters.SelectedItems[0].SubItems[2].Text;
            frm.text4 = ListViewCounters.SelectedItems[0].SubItems[3].Text;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ListViewCounters.SelectedItems[0].SubItems[0].Text = frm.text1;
                ListViewCounters.SelectedItems[0].SubItems[1].Text = frm.text2;
                ListViewCounters.SelectedItems[0].SubItems[2].Text = frm.text3;
                ListViewCounters.SelectedItems[0].SubItems[3].Text = frm.text4;
            }
        }

        private void ButtonRemoveLaser_Click(object sender, EventArgs e)
        {
            if (ListViewLasers.SelectedItems.Count != 1) return;
            if (MessageBox.Show("Are you sure?", "Remove Laser", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ListViewLasers.Items.RemoveAt(ListViewLasers.SelectedIndices[0]);
        }

        private void ButtonRemoveCounter_Click(object sender, EventArgs e)
        {
            if (ListViewCounters.SelectedItems.Count != 1) return;
            if (MessageBox.Show("Are you sure?", "Remove Counter", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ListViewCounters.Items.RemoveAt(ListViewCounters.SelectedIndices[0]);
        }

        private void ButtonSaveFilmingSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PMASavePath = TextBoxPMAPath.Text;
            Properties.Settings.Default.PMAhead = TextBoxPMAHead.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings are successfully saved.", "Settings");
            LoadAllSettings();
        }

        private void ButtonSaveCCDSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.BinSizeIndex = ComboBoxBinSize.SelectedIndex;
            Properties.Settings.Default.ZoomModeIndex = ComboBoxZoomMode.SelectedIndex;
            Properties.Settings.Default.CCDModelIndex = ComboBoxCCDModel.SelectedIndex;
            Properties.Settings.Default.ImagingWidth = (int)NUDImagingWidth.Value;
            Properties.Settings.Default.ImagingHeight = (int)NUDImagingHeight.Value;
            Properties.Settings.Default.CameraIndex = (int)NUDCameraIndex.Value;
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings are successfully saved.", "Settings");
            LoadAllSettings();
        }

        private void ButtonSaveDAQSettings_Click(object sender, EventArgs e)
        {
            System.Collections.Specialized.StringCollection lasers = new System.Collections.Specialized.StringCollection();
            System.Collections.Specialized.StringCollection counters = new System.Collections.Specialized.StringCollection();

            for (int i = 0; i < ListViewLasers.Items.Count; i ++)
            {
                lasers.Add(ListViewLasers.Items[i].SubItems[0].Text);
                lasers.Add(ListViewLasers.Items[i].SubItems[1].Text);
            }


            for (int i = 0; i < ListViewCounters.Items.Count; i++)
            {
                counters.Add(ListViewCounters.Items[i].SubItems[0].Text);
                counters.Add(ListViewCounters.Items[i].SubItems[1].Text);
                counters.Add(ListViewCounters.Items[i].SubItems[2].Text);
                counters.Add(ListViewCounters.Items[i].SubItems[3].Text);
            }
            Properties.Settings.Default.Lasers = lasers;
            Properties.Settings.Default.Counters = counters;
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings are successfully saved.", "Settings");
            LoadAllSettings();
        }
        #endregion

        #region Auto Focusing
        private void ButtonAFConnect_Click(object sender, EventArgs e)
        {
            if (ComboBoxAFDevices.Text == "")
            {
                MessageBox.Show("Select a Device.", "Single 2013");
                return;
            }
            try
            {
                if (m_stage == null)
                    m_stage = new smbStage((smbStage.StageType)ComboBoxAFDevices.SelectedIndex);
                m_autofocusing = new AutoFocusing(m_stage, m_ccd, this, m_imgdrawer, (int)NUDAFRange.Value);
            } catch (Exception) {
                MessageBox.Show("Initialization Failed! Did you turned your device on?", "Single 2013");
                return;
            }
            CheckBoxAFIgnoreDarkFrame.Enabled = true;
            CheckBoxAFKalman.Enabled = true;
            ButtonAFCalibration.Enabled = true;
        }

        private void ButtonAFStart_Click(object sender, EventArgs e)
        {
            ButtonAFCalibration.Enabled = m_autofocusing.m_focusing;
            if (m_autofocusing.m_focusing)
            {
                m_autofocusing.StopFocusing();
                ButtonAFStart.Text = "Start Focusing";
            }
            else
            {
                ChartAFDistance.Series.Clear();
                ChartAFFOM.Series.Clear();

                ChartAFDistance.Series.Add("Distance");
                ChartAFFOM.Series.Add("FOM");

                ChartAFDistance.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                ChartAFFOM.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                m_autofocusing.SetCalibration(Convert.ToDouble(TextBoxAFSlope.Text), Convert.ToDouble(TextBoxAFStdev.Text));
                m_autofocusing.StartFocusing();
                ButtonAFStart.Text = "Stop Focusing";
            }
        }

        private void ButtonAFCalibration_Click(object sender, EventArgs e)
        {
            ButtonAFCalibration.Enabled = false;
            Log("[Auto Focusing]", new string[] { "Calibrating..." });
            m_autofocusing.Calibrate(m_ccd.m_exptime);
        }

        private void ButtonAFLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.ShowDialog();

            if (d.FileName != "")
            {
                string[] lines = System.IO.File.ReadAllLines(d.FileName);
                if (lines.Length < 2) return;
                TextBoxAFSlope.Text = lines[0];
                TextBoxAFStdev.Text = lines[1];
            }
        }

        private void ButtonAFSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.ShowDialog();

            if (d.FileName != "")
            {
                System.IO.File.WriteAllLines(d.FileName, new string[] { TextBoxAFSlope.Text, TextBoxAFStdev.Text });
            }
        }

        private void CheckBoxAFKalman_CheckedChanged(object sender, EventArgs e)
        {
            if (m_autofocusing != null)
                m_autofocusing.m_applyingkalman = CheckBoxAFKalman.Checked;
        }

        private void CheckBoxAFIgnoreDarkFrame_CheckedChanged(object sender, EventArgs e)
        {
            if (m_autofocusing != null && CheckBoxAFIgnoreDarkFrame.Checked)
                m_autofocusing.m_ignoredarkframe = ALEXCheckedListBox.CheckedItems.Count;
        }

        private void NUDAFRange_ValueChanged(object sender, EventArgs e)
        {
            if (m_autofocusing != null && NUDAFRange.Value <= NUDChannelNum.Value)
                m_autofocusing.m_selectedchannel = (int)NUDAFRange.Value;
        }
        #endregion

        #region Auto Flow
        private void ButtonAFLFindDevices_Click(object sender, EventArgs e)
        {
            ButtonAFLFindDevices.Enabled = false;
            if (m_autoflow == null)
            {
                m_autoflow = new AutoFlow(m_ccd, m_imgdrawer, this);
            }
            else
            {
                m_autoflow.m_autoflow = false;
                if (m_autoflow.m_autoflow)
                    ButtonAFLEnable_Click(sender, e);
            }
            
            ListViewAFLPumps.Items.Clear();
            foreach (smbPump pump in m_autoflow.m_pumps)
                ListViewAFLPumps.Items.Add(new ListViewItem(new string[] { smbPump.GetPumpName(pump.m_pump), pump.m_port }));
            ButtonAFLEnable.Enabled = true;
            ButtonAFLFindDevices.Enabled = true;
        }

        private void ListViewAFLPumps_Click(object sender, EventArgs e)
        {
            if (ListViewAFLPumps.SelectedIndices.Count != 1) return;
            m_selectedpump = ListViewAFLPumps.SelectedIndices[0];
            NUDAFLRate.Maximum = (int)m_autoflow.m_pumps[m_selectedpump].m_maxrate;
            NUDAFLRate.Minimum = (int)m_autoflow.m_pumps[m_selectedpump].m_minrate;
            TrackBarAFLRate.Maximum = (int)NUDAFLRate.Maximum;
            TrackBarAFLRate.Minimum = (int)NUDAFLRate.Minimum;
            NUDAFLVolume.Maximum = (int)m_autoflow.m_pumps[m_selectedpump].m_maxvolume;
            NUDAFLVolume.Minimum = (int)m_autoflow.m_pumps[m_selectedpump].m_minvolume;

            TextBoxAFLDiameter.Text = m_autoflow.m_pumps[m_selectedpump].GetDiameter().ToString();
            NUDAFLRate.Value = (int)m_autoflow.m_pumps[m_selectedpump].GetRate();
            NUDAFLVolume.Value = (int)m_autoflow.m_pumps[m_selectedpump].GetVolume();
            if (m_autoflow.m_pumps[m_selectedpump].GetDirection() == smbPump.runMode.INFUSION)
            {
                RadioButtonAFLInfusion.Checked = true;
                RadioButtonAFLRefill.Checked = false;
            }
            else
            {
                RadioButtonAFLInfusion.Checked = false;
                RadioButtonAFLRefill.Checked = true;
            }
            
        }

        private void TrackBarAFLRate_Scroll(object sender, EventArgs e)
        {
            NUDAFLRate.Value = TrackBarAFLRate.Value;
        }

        private void NUDAFLRate_ValueChanged(object sender, EventArgs e)
        {
            TrackBarAFLRate.Value = (int)NUDAFLRate.Value;
        }

        private void ButtonAFLInstantRun_Click(object sender, EventArgs e)
        {
            if (m_selectedpump == -1) return;
            m_autoflow.InstantFlow(m_selectedpump, Convert.ToDouble(TextBoxAFLDiameter.Text), Convert.ToDouble(NUDAFLVolume.Value), Convert.ToDouble(NUDAFLRate.Value), RadioButtonAFLInfusion.Checked ? smbPump.runMode.INFUSION : smbPump.runMode.REFILL);
        }

        private void ButtonAFLAddRule_Click(object sender, EventArgs e)
        {
            if (m_selectedpump == -1) return;
            ListViewAFLRules.Items.Add(new ListViewItem(new string[] { NUDAFLFrameNumber.Value.ToString(), ListViewAFLPumps.Items[m_selectedpump].SubItems[0].Text + " (" + ListViewAFLPumps.Items[m_selectedpump].SubItems[1].Text + ")", TextBoxAFLDiameter.Text, NUDAFLVolume.Value.ToString(), NUDAFLRate.Value.ToString(), RadioButtonAFLInfusion.Checked ? "Infusion" : "Refill" })).Tag = m_selectedpump;
        }

        private void ButtonAFLRemoveRules_Click(object sender, EventArgs e)
        {
            if (ListViewAFLRules.SelectedIndices.Count == 0) return;
            ListViewAFLRules.Items.RemoveAt(ListViewAFLRules.SelectedIndices[0]);
        }

        private void ButtonAFLEnable_Click(object sender, EventArgs e)
        {
            ButtonAFLAddRule.Enabled = m_autoflow.m_autoflow;
            ButtonAFLRemoveRules.Enabled = m_autoflow.m_autoflow;
            ListViewAFLRules.Enabled = m_autoflow.m_autoflow;

            if (m_autoflow.m_autoflow)
            {
                m_autoflow.StopAutoFlow();
                m_autoflow.ClearRules();
                ButtonAFLEnable.Text = "Enable Auto Flow";
                ListViewAFLRules.ItemCheck -= new ItemCheckEventHandler(ListViewAFLRules_ItemCheck);
                foreach (ListViewItem x in ListViewAFLRules.Items)
                    x.Checked = false;
                ListViewAFLRules.ItemCheck += new ItemCheckEventHandler(ListViewAFLRules_ItemCheck);
                Log("[Auto Flow]", new string[] { "Auto flow has been disabled." });
            }
            else
            {
                ButtonAFLEnable.Text = "Disable Auto Flow";
                foreach (ListViewItem x in ListViewAFLRules.Items)
                    m_autoflow.AddFlowRule((int)x.Tag, Convert.ToInt32(x.SubItems[0].Text), Convert.ToDouble(x.SubItems[2].Text), Convert.ToDouble(x.SubItems[3].Text), Convert.ToDouble(x.SubItems[4].Text), x.SubItems[5].Text == "Infusion" ? smbPump.runMode.INFUSION : smbPump.runMode.REFILL);
                m_autoflow.StartAutoFlow();
                Log("[Auto Flow]", new string[] { "Auto flow is Enabled!" });
            }
        }

        private void ListViewAFLRules_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            e.NewValue = e.CurrentValue;
        }
        #endregion

        #region Active Drift Correction
        private void ButtonADCInitialize_Click(object sender, EventArgs e)
        {
            double[] ref_pos = { 0, 0 };

            MessageBox.Show(this, "Please remove prism NOW! And reinstall it after initialization.\r\n\r\nPress OK button to continue...", "Warning!");
            try
            {
                m_adc = new ActiveDriftCorrection((int)NUDADCPiezomirrorNum.Value, ref_pos, m_imgdrawer, PictureBoxADCPinhole1, PictureBoxADCPinhole2, PictureBoxADCObject);
            }
            catch
            {
                MessageBox.Show("Initialization Failed! Did you turned your device on?", "Single 2013");
                return;
            }

            for (int i = 0; i < (int)NUDADCPiezomirrorNum.Value; i++)
            {
                ComboADCMirrorNum.Items.Add(i + 1);
            }
            showXYZstagePos();

            ButtonADCInitialize.Enabled = false;
            ButtonADCSelectPinhole1.Enabled = true;
            ButtonADCSelectPinhole2.Enabled = true;
            ButtonADCSelectObject.Enabled = true;
            ButtonADCStart.Enabled = true;
        }

        private void ButtonADCIncreaseX_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffXYZstage(Convert.ToDouble(TextBoxADCNanostageStepSize.Text), 1);
            showXYZstagePos();
        }

        private void ButtonADCDecreaseX_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffXYZstage(-Convert.ToDouble(TextBoxADCNanostageStepSize.Text), 1);
            showXYZstagePos();
        }

        private void ButtonADCIncreaseY_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffXYZstage(-Convert.ToDouble(TextBoxADCNanostageStepSize.Text), 2);
            showXYZstagePos();
        }

        private void ButtonADCDecreaseY_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffXYZstage(Convert.ToDouble(TextBoxADCNanostageStepSize.Text), 2);
            showXYZstagePos();
        }

        private void showXYZstagePos()
        {
            double[] pos = m_adc.getCurrentPosXYZstage();
            LabelADCCurrentPos.Text = string.Format("X: {0:0.000} um\r\nY: {1:0.000} um\r\nZ: {2:0.000} um", pos[0], pos[1], pos[2]);
        }

        private void ButtonADCIncreaseTheta_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffPiezomirror(-Convert.ToDouble(TextBoxADCPiezomirrorStepSize.Text), 2);
            showPiezomirrorPos();
        }

        private void ButtonADCDecreaseTheta_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffPiezomirror(Convert.ToDouble(TextBoxADCPiezomirrorStepSize.Text), 2);
            showPiezomirrorPos();
        }

        private void ButtonADCIncreasePhi_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffPiezomirror(Convert.ToDouble(TextBoxADCPiezomirrorStepSize.Text), 1);
            showPiezomirrorPos();
        }

        private void ButtonADCDecreasePhi_Click(object sender, EventArgs e)
        {
            m_adc.moveDiffPiezomirror(-Convert.ToDouble(TextBoxADCPiezomirrorStepSize.Text), 1);
            showPiezomirrorPos();
        }

        private void showPiezomirrorPos()
        {
            double[] angle = m_adc.getCurrentAnglePiezomirror();
            LabelADCCurrentAngle.Text = string.Format("θ: {0:0.000} mrad\r\nφ: {1:0.000} mrad", angle[0], angle[1]);
        }

        private void ComboADCMirrorNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_adc.setPiezomirrorIndex(ComboADCMirrorNum.SelectedIndex);
            showPiezomirrorPos();
        }
        
        private void buttonADCSelectPinhole1_Click(object sender, EventArgs e)
        {
            m_adc.setXY(0, cursorXY);
        }

        private void ButtonADCSelectPinhole2_Click(object sender, EventArgs e)
        {
            m_adc.setXY(1, cursorXY);
        }

        private void ButtonADCSelectObject_Click(object sender, EventArgs e)
        {
            m_adc.setXY(2, cursorXY);
        }

        private void ButtonADCStart_Click(object sender, EventArgs e)
        {
            if (m_adc.m_activedriftcorrection)
            {
                m_adc.StopADC();
                ButtonADCStart.Text = "Start Active Drift Correction";
                GroupBoxManualNanostage.Enabled = true;
                GroupBoxManualPiezomirrors.Enabled = true;
            }
            else
            {
                m_adc.StartADC();
                ButtonADCStart.Text = "Stop Active Drift Correction";
                GroupBoxManualNanostage.Enabled = false;
                GroupBoxManualPiezomirrors.Enabled = false;
            }
        }
        #endregion
    }
}
