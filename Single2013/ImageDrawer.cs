
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SMBdevices;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Single2013
{
    public class ImageDrawer
    {
        uint[,] colortable = new uint[256, 3];

        public Semaphore dumparray_sem = new Semaphore(1, 1);
        public Semaphore displayarray_sem = new Semaphore(1, 1);

        public byte[,] dump_array;
        public int[] display_array;

        public bool AFFlag = false;

        private smbCCD m_ccd;
        private int[] m_buf;
        private bool m_drawflag = false;
        private Thread m_drawingThread;
        private PictureBox m_pb;

        private frmTIRF m_frm;
        public bool m_filming = false;

        private int m_max = 0;
        private int[] m_min;

        private double m_scaler;
        private int[] m_subs;

        private FileStream m_filestream;
        private BinaryWriter m_bw;
        //private string m_pmafilename;

        public int m_framenum;
        public bool m_auto;

        public int m_autostopframenum = -1;
        public List<string> m_autoflowcommands = new List<string>();

        private bool m_guidelines;
        public int[,] ADCCursorXYs = new int[3, 2] {{-1, -1}, {-1, -1}, {-1, -1}};

        private void LoadColors(string path)
        {
            int i;

            StreamReader sr = new StreamReader(path);
            int counter = 0;
            string line;
            int n;
            while (sr.Peek() >= 0)
            {
                line = sr.ReadLine();
                string[] words = line.Split(' ');
                n = 0;
                for (i = 0; i < words.Length; i++)
                {
                    if (words[i] != "")
                        colortable[counter, n++] = Convert.ToUInt32(words[i].Trim());
                }
                counter++;
            }

            for (i = counter; i < 256; i++)
            {
                colortable[i, 0] = 0;
                colortable[i, 1] = 0;
                colortable[i, 2] = 0;
            }
        }

        private int ColorMaker(uint input)
        {
            int rtn = (int)colortable[input, 2];
            rtn += (int)(colortable[input, 1] * 256);
            rtn += (int)(colortable[input, 0] * 256 * 256);
            return rtn;
        }

        private void CalMaxMin()
        {
            int i, j, k;

            m_min = new int[m_frm.m_chanum];
            for (i = 0; i < m_min.Length; i++) m_min[i] = 100000000;
            m_max = 0;

            for (j = m_ccd.m_clipsize; j < m_ccd.m_imageheight - m_ccd.m_clipsize; j++)
            {
                for (k = 0; k < m_frm.m_chanum; k++)
                {
                    for (i = k * m_ccd.m_imagewidth / m_frm.m_chanum + m_ccd.m_clipsize; i < (k + 1) * m_ccd.m_imagewidth / m_frm.m_chanum - m_ccd.m_clipsize; i++)
                    {
                        if (m_buf[j * m_ccd.m_imagewidth + i] > m_max)
                            m_max = m_buf[j * m_ccd.m_imagewidth + i]; //search the max intensity
                        if (m_buf[j * m_ccd.m_imagewidth + i] < m_min[k])
                            m_min[k] = m_buf[j * m_ccd.m_imagewidth + i]; //search the min intensity 
                    }
                }
            }
        }

        private void DrawingThread()
        {
            int i, j, k, t;
            int high = m_ccd.m_imagewidth * m_ccd.m_imageheight;

            while (m_drawflag)
            {
                Thread.Sleep(0);
                try
                {
                    m_ccd.GetImage(m_buf);
                }
                catch { 
                    m_drawflag = false;
                    return;
                }

                if (m_auto)
                {
                    // Direct insertion of the values in a thread doesn't work.
                    // Instead, we should use 'Invoke' method to put the values to the controls...
                    CalMaxMin();
                    try
                    {
                        m_frm.Invoke(new frmTIRF.updateAutoScaleInfoDelegate(m_frm.updateAutoScaleInfo), new object[] { m_max, m_min });
                    } catch {}
                    m_subs = m_min;
                    m_scaler = ((double)(m_max - m_min[0])) / 256;
                    if (m_scaler < 1) m_scaler = 1;
                }

                dumparray_sem.WaitOne();
                displayarray_sem.WaitOne();
                for (j = 0; j < m_ccd.m_imageheight; j++)
                {
                    for (k = 0; k < m_frm.m_chanum; k++)
                    {
                        for (i = k * m_ccd.m_imagewidth / m_frm.m_chanum; i < (k + 1) * m_ccd.m_imagewidth / m_frm.m_chanum; i++)
                        {
                            t = high - (j * m_ccd.m_imagewidth + (m_ccd.m_imagewidth - i));
                            dump_array[j, i] = (byte)(((m_buf[t] - m_subs[k]) / m_scaler));
                            if (m_buf[t] - m_subs[k] < 0) dump_array[j, i] = 0;
                            else if ((m_buf[t] - m_subs[k]) / m_scaler > 255) dump_array[j, i] = 255;
                            display_array[j * m_ccd.m_imagewidth + i] = ColorMaker(dump_array[j, i]);
                        }
                    }
                }

                if (m_guidelines)
                {
                    for (k = 0; k < m_frm.m_chanum; k++)
                    {
                        for (j = 0; j < m_ccd.m_imageheight; j++)
                        {
                            display_array[m_ccd.m_imagewidth * j + m_ccd.m_imagewidth / m_frm.m_chanum * k] = 166 + 97 * 256 + 243 * 256 * 256;
                        }
                    }
                }

                if (m_frm.cursorXY[0] != -1 && m_frm.cursorXY[1] != -1)
                {
                    for (k = m_frm.cursorXY[0] - 5; k < m_frm.cursorXY[0] + 6; k++)
                    {
                        display_array[m_ccd.m_imagewidth * k + m_frm.cursorXY[1] + 5] = 255 + 255 * 256 + 255 * 256 * 256;
                        display_array[m_ccd.m_imagewidth * k + m_frm.cursorXY[1] - 5] = 255 + 255 * 256 + 255 * 256 * 256;
                    }
                    for (k = m_frm.cursorXY[1] - 5; k < m_frm.cursorXY[1] + 6; k++)
                    {
                        display_array[m_ccd.m_imagewidth * (m_frm.cursorXY[0] + 5) + k] = 255 + 255 * 256 + 255 * 256 * 256;
                        display_array[m_ccd.m_imagewidth * (m_frm.cursorXY[0] - 5) + k] = 255 + 255 * 256 + 255 * 256 * 256;
                    }
                }

                for (j = 0; j < 3; j++)
                {
                    if (ADCCursorXYs[j, 0] != -1 && ADCCursorXYs[j, 1] != -1)
                    {
                        for (k = ADCCursorXYs[j, 0] - 4; k < ADCCursorXYs[j, 0] + 5; k++)
                        {
                            display_array[m_ccd.m_imagewidth * k + ADCCursorXYs[j, 1] + 4] = 255 * 256 + 255 * 256 * 256;
                            display_array[m_ccd.m_imagewidth * k + ADCCursorXYs[j, 1] - 4] = 255 * 256 + 255 * 256 * 256;
                        }
                        for (k = ADCCursorXYs[j, 1] - 4; k < ADCCursorXYs[j, 1] + 5; k++)
                        {
                            display_array[m_ccd.m_imagewidth * (ADCCursorXYs[j, 0] + 4) + k] = 255 * 256 + 255 * 256 * 256;
                            display_array[m_ccd.m_imagewidth * (ADCCursorXYs[j, 0] - 4) + k] = 255 * 256 + 255 * 256 * 256;
                        }
                    }
                }

                displayarray_sem.Release();
                AFFlag = false;

                if (m_filming)
                {
                    m_framenum++;
                    m_frm.Invoke(new frmTIRF.updateFilmingInfoDelegate(m_frm.updateFilmingInfo), new object[] { m_framenum });
                    for (i = 0; i < m_ccd.m_imageheight; i++)
                        for (j = 0; j < m_ccd.m_imagewidth; j++)
                            m_bw.Write(dump_array[i, j]);

                    if (m_autostopframenum > 0 && m_autostopframenum == m_framenum)
                        m_frm.Invoke(new frmTIRF.AutoStopDelegate(m_frm.AutoStop), new object[] { });
                }
                dumparray_sem.Release();

                if (m_framenum % 1 == 0)
                {
                    Bitmap bitmap;
                    displayarray_sem.WaitOne();
                    unsafe
                    {
                        fixed (int* intPtr = &display_array[0])
                        {
                            bitmap = new Bitmap(m_ccd.m_imagewidth, m_ccd.m_imageheight, 4 * ((m_ccd.m_imagewidth * 4 + 3) / 4), PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                        }
                    }
                    displayarray_sem.Release();
                    try
                    {
                        m_frm.Invoke(new frmTIRF.DrawFrameDelegate(m_frm.DrawFrame), new object[] { bitmap });
                    }
                    catch { }
                }
            }
        }

        public void StartFilming(string pmafilename)
        {
            m_filestream = new FileStream(pmafilename, FileMode.Append, FileAccess.Write, FileShare.Write);
            m_bw = new BinaryWriter(m_filestream);
            m_filming = true;
        }

        public void StopFilming()
        {
            m_filming = false;
            m_bw.Close();
            m_filestream.Close();
        }

        public ImageDrawer(string colormappath, frmTIRF f, smbCCD ccd)
        {
            m_frm = f;
            m_ccd = ccd;
            m_auto = false;
            m_scaler = 1;

            LoadColors(colormappath);
            m_buf = new int[m_ccd.m_imagewidth * m_ccd.m_imageheight];

            dump_array = new byte[m_ccd.m_imageheight, m_ccd.m_imagewidth];
            display_array = new int[m_ccd.m_imagewidth * m_ccd.m_imageheight];
        }

        public void SetValues(double scaler, int[] subs)
        {
            m_scaler = scaler;
            m_subs = subs;
        }
        ~ImageDrawer()
        {
            m_drawflag = false;
            if (m_drawingThread != null) m_drawingThread.Join();
        }

        public void StartDrawing(PictureBox pb, smbCCD ccd)
        {
            m_ccd = ccd;
            m_pb = pb;

            m_drawflag = true;
            m_drawingThread = new Thread(new ThreadStart(DrawingThread));
            m_drawingThread.Priority = ThreadPriority.Normal;
            m_drawingThread.IsBackground = true;
            m_drawingThread.Start();
        }

        public void StopDrawing()
        {
            m_drawflag = false;
        }

        public void ToggleGuidelines(bool guidestatus)
        {
            m_guidelines = guidestatus;
        }
    }
}
