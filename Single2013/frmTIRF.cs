using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using SMBdevices;

namespace Single2013
{
    public partial class frmTIRF : Form
    {
        private smbCCD m_ccd;
        private smbShutter m_shutter;
        private smbPiezo m_piezo;

        private ImageDrawer m_imgdrawer;
        private AutoFocusing m_autofocusing;

        public bool m_CCDon = false;

        private string m_pmafiledir;
        private string m_pmafilenamehead;

        public frmTIRF()
        {
            InitializeComponent();
        }

        #region Delegates For MultiThreading
        public delegate void updateAutoScaleInfoDelegate(int theMax, int[] theMin);
        public void updateAutoScaleInfo(int theMax, int[] theMin)
        {
            double scaler = (double)(theMax - theMin[0]) / 256;
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

        public delegate void updateAFInfoDelegate(string info);
        public void updateAFInfo(string info)
        {
            LabelAFInfo.Text = info;
        }
        #endregion

        #region Helper Methods
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

        private void LoadAllSettings()
        {
            // Settings Related
            ComboBoxBinSize.SelectedIndex = Properties.Settings.Default.BinSizeIndex;
            ComboBoxZoomMode.SelectedIndex = Properties.Settings.Default.ZoomModeIndex;

            CommonData.binsize = Convert.ToInt32(ComboBoxBinSize.Items[Properties.Settings.Default.BinSizeIndex]);
            m_ccd.SetBinSize(CommonData.binsize, CommonData.binsize);
            CommonData.imageheight = 512 / CommonData.binsize;
            CommonData.imagewidth = 512 / CommonData.binsize;
            CommonData.clipsize = 30 / (512 / Math.Min(CommonData.imagewidth, CommonData.imageheight));

            if (ComboBoxZoomMode.SelectedIndex == 0) //Center
            {
                CCDWindow.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                CCDWindow.SizeMode = PictureBoxSizeMode.StretchImage;
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
            for (int i = 0; i < Properties.Settings.Default.Lasers.Count; i += 2)
                AddLaserToListBox(Properties.Settings.Default.Lasers[i], Properties.Settings.Default.Lasers[i + 1]);

            ALEXCheckedListBox.Items.Clear();
            for (int i = 0; i < Properties.Settings.Default.Counters.Count; i += 4)
                AddCounterBoardToListBox(Properties.Settings.Default.Counters[i],
                                         Properties.Settings.Default.Counters[i + 1],
                                         Properties.Settings.Default.Counters[i + 2],
                                         Properties.Settings.Default.Counters[i + 3]);

            // Drawer Class
            m_imgdrawer = new ImageDrawer(Properties.Settings.Default.Colortable, this);

            // PMA file path setting
            m_pmafiledir = Properties.Settings.Default.PMASavePath;
            m_pmafilenamehead = Properties.Settings.Default.PMAhead;

        }
        #endregion

        #region Control Events
        private void frmTIRF_Load(object sender, EventArgs e)
        {
            LogTextBox.Text = "Single 2013 - TIRF by Jeongbin Park";

            // Instruments
            m_ccd = new smbCCD(smbCCD.CCDType.ENDOR_CCD);
            m_ccd.SetTemp(-85);

            m_shutter = new smbShutter(smbShutter.ShutterType.NI_DAQ);

            LoadAllSettings();

            OffAllLaser();

            NUDChannelNum.Value = 2;
            SetGainButton_Click(sender, e);

            LogTextBox.Text += "\r\nReady.";
            Log("Temperature Warning:", new string[] { "Temperature is set to " + Convert.ToString(-85) + " C." });
        }

        private void frmTIRF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_ccd.GetTemp() < -20)
            {
                MessageBox.Show("In order to shutdown whole system, Temperature of CCD should be above -20 C.", "Single 2013");
                e.Cancel = true;
            }
            m_imgdrawer.m_auto = false;
            m_imgdrawer.m_filming = false;
            m_ccd.m_gettingimage = false;
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
                Log("Shutter Opened.", new string[] { "Bin Size: " + CommonData.binsize.ToString(), "Stretch Mode: " + CCDWindow.SizeMode.ToString() });
            }
            else
            {
                if (m_imgdrawer.m_filming) StartFilmingButton_Click(sender, e);
                m_imgdrawer.StopDrawing();
                m_ccd.ShutterOff();
                OpenCameraButton.Text = "Open Camera";
                Log("Shutter Closed", new string[] {});
            }
            StartFilmingButton.Enabled = !m_CCDon;
            GroupBoxCCDSettings.Enabled = m_CCDon;
            GroupBoxDAQSettings.Enabled = m_CCDon;
            SetGainButton.Enabled = m_CCDon;
            NUDChannelNum.Enabled = m_CCDon;
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
            CommonData.chanum = (int)NUDChannelNum.Value;
        }

        private void StartFilmingButton_Click(object sender, EventArgs e)
        {
            if (m_imgdrawer.m_filming) {
                StartFilmingButton.Text = "Start Filming";
                m_shutter.StopALEX();
                LaserCheckedListBox.Enabled = true;
                for (int i = 0; i < LaserCheckedListBox.Items.Count; i++)
                {
                    if (LaserCheckedListBox.GetItemChecked(i)) m_shutter.LaserOn(i);
                    else m_shutter.LaserOff(i);
                }
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
                    file.WriteLine("Acquisition mode:  Full " + CommonData.imagewidth.ToString() + "x" + CommonData.imageheight.ToString() + " " + CommonData.binsize.ToString() + "x" + CommonData.binsize.ToString() + " Binning");
                    file.WriteLine("Gain:  " + NUDGain.Value.ToString());
                    file.WriteLine("Data scaler:  " + TextBoxScaler.Text);
                    string str = "Background subtraction:";
                    foreach (NumericUpDown sub in SplitConSubs.Panel2.Controls)
                        str += "  " + sub.Value.ToString();
                    file.WriteLine(str);
                }

                m_imgdrawer.m_pmafilename = filename;
                Log("Filming Started.", new string [] { "File Name: " + filename });
                StartFilmingButton.Text = "Stop Filming";
                using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var bw = new BinaryWriter(fileStream))
                {
                    bw.Write(BitConverter.GetBytes((short)CommonData.imagewidth));
                    bw.Write(BitConverter.GetBytes((short)CommonData.imageheight));
                }
                if (ALEXCheckedListBox.CheckedItems.Count > 1)
                {
                    LaserCheckedListBox.Enabled = false;
                    m_shutter.StartALEX(m_ccd.m_exptime);
                }
            }
            m_imgdrawer.m_filming = !m_imgdrawer.m_filming;
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
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings are successfully saved.", "Settings");
            LoadAllSettings();
            SetGainButton_Click(sender, e);
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

        #region Auto Focusing Events
        private void ButtonAFConnect_Click(object sender, EventArgs e)
        {
            m_piezo = new smbPiezo(smbPiezo.PiezoType.MCL_CFOCUS);
            m_autofocusing = new AutoFocusing(m_piezo);
        }
        #endregion

        private void ButtonStartFocusing_Click(object sender, EventArgs e)
        {
            if (m_autofocusing.m_focusing)
                m_autofocusing.StopFocusing();
            else
                m_autofocusing.StartFocusing(1, this);
        }
    }
}
