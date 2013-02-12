
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

        public static Semaphore dumparray_sem = new Semaphore(1, 1);
        public static Semaphore displayarray_sem = new Semaphore(1, 1);

        public static byte[,] dump_array;
        public static int[] display_array;

        public static bool isArraydumped = false; //af

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

        public int m_framenum;
        public bool m_auto;

        public string m_pmafilename;
        
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
                for (i=0; i<words.Length; i++)
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

            m_min = new int[CommonData.chanum];
            for (i = 0; i < m_min.Length; i++) m_min[i] = 100000000;
            m_max = 0;

            for (j = CommonData.clipsize; j < CommonData.imageheight - CommonData.clipsize; j++)
            {
                for (k = 0; k < CommonData.chanum; k++)
                {
                    for (i = k * CommonData.imagewidth / CommonData.chanum + CommonData.clipsize; i < (k + 1) * CommonData.imagewidth / CommonData.chanum - CommonData.clipsize; i++)
                    {
                        if (m_buf[j * CommonData.imagewidth + i] > m_max)
                            m_max = m_buf[j * CommonData.imagewidth + i]; //search the max intensity
                        if (m_buf[j * CommonData.imagewidth  + i] < m_min[k])
                            m_min[k] = m_buf[j * CommonData.imagewidth + i]; //search the min intensity 
                    }
                }
            }            
        }

        private void DrawingThread()
        {
            int i, j, k;
            int high = CommonData.imagewidth * CommonData.imageheight;

            while (m_drawflag)
            {
                //try
                //{
                    m_ccd.m_gettingimage = true;
                    m_ccd.GetImage(m_buf);
                    m_ccd.m_gettingimage = false;
                    CalMaxMin();

                    if (m_auto)
                    {
                        // Direct insert of the values in a thread doesn't work.
                        // Instead, we should use 'Invoke' method to put the values to the controls...
                        m_frm.Invoke(new frmTIRF.updateAutoScaleInfoDelegate(m_frm.updateAutoScaleInfo), new object[] { m_max, m_min });
                        m_subs = m_min;
                        m_scaler = ((double)(m_max - m_min[0])) / 256;
                        if (m_scaler < 1) m_scaler = 1;
                    }

                    dumparray_sem.WaitOne();
                    displayarray_sem.WaitOne();
                    for (j = 0; j < CommonData.imageheight; j++)
                    {
                        for (k = 0; k < CommonData.chanum; k++)
                        {
                            for (i = k * CommonData.imagewidth / CommonData.chanum; i < (k + 1) * CommonData.imagewidth / CommonData.chanum; i++)
                            {
                                dump_array[j, i] = (byte)((double)((m_buf[high - (j * CommonData.imagewidth + (CommonData.imageheight - i))] - m_subs[k]) / m_scaler));
                                if (m_buf[high - (j * CommonData.imagewidth + (CommonData.imageheight - i))] - m_subs[k] < 0) dump_array[j, i] = 0;
                                if ((m_buf[high - (j * CommonData.imagewidth + (CommonData.imageheight - i))] - m_subs[k]) / m_scaler > 255) dump_array[j, i] = 255;
                                display_array[j * CommonData.imagewidth + i] = ColorMaker(dump_array[j, i]);
                            }
                        }
                    }
                    displayarray_sem.Release();
                    isArraydumped = true;

                    if (m_filming)
                    {
                        m_framenum++;
                        m_frm.Invoke(new frmTIRF.updateFilmingInfoDelegate(m_frm.updateFilmingInfo), new object[] { m_framenum });
                        using (var fileStream = new FileStream(m_pmafilename, FileMode.Append, FileAccess.Write, FileShare.None))
                        using (var bw = new BinaryWriter(fileStream))
                        {
                            for (i = 0; i < CommonData.imagewidth; i++)
                                for (j = 0; j < CommonData.imageheight; j++)
                                    bw.Write(dump_array[i, j]);
                        }
                    }
                    dumparray_sem.Release();

                    Bitmap bitmap;
                    displayarray_sem.WaitOne();
                    unsafe
                    {
                        fixed (int* intPtr = &display_array[0])
                        {
                            bitmap = new Bitmap(CommonData.imagewidth, CommonData.imageheight, 4 * ((CommonData.imagewidth * 4 + 3) / 4), PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                        }
                    }
                    m_pb.Image = bitmap;
                    displayarray_sem.Release();
                //}
                //catch { }
            }
        }

        public ImageDrawer(string colormappath, frmTIRF f)
        {
            LoadColors(colormappath);
            m_buf = new int[CommonData.imagewidth * CommonData.imageheight];
            
            m_auto = false;
            m_scaler = 1;
            dump_array = new byte[CommonData.imagewidth, CommonData.imageheight];
            display_array = new int[CommonData.imagewidth * CommonData.imageheight];
            m_frm = f;
        }

        public void SetValues(double scaler, int[] subs)
        {
            m_scaler = scaler;
            m_subs = subs;
        }
        ~ImageDrawer()
        {
            m_drawflag = false;
            if ( m_drawingThread != null ) m_drawingThread.Join();

        }

        public void StartDrawing(PictureBox pb, smbCCD ccd)
        {
            m_ccd = ccd;
            m_pb = pb;

            m_drawflag = true;
            m_drawingThread = new Thread(new ThreadStart(DrawingThread));
            m_drawingThread.Priority = ThreadPriority.AboveNormal;
            m_drawingThread.Start();
        }

        public void StopDrawing()
        {
            m_drawflag = false;
            if (m_ccd != null) m_ccd.m_gettingimage = false;
        }
    }
}
