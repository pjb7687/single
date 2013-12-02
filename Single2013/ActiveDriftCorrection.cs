using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMBdevices;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace Single2013
{
    class ActiveDriftCorrection
    {
        private List<smbStage> m_piezomirrors = new List<smbStage> ();
        private smbStage m_xyzstage;
        private ImageDrawer m_imgdrawer;
        private Thread m_adcThread;

        public PictureBox[] m_pbs = new PictureBox[3];

        private double[] m_refpos;

        private int m_piezoindex;
        public bool m_activedriftcorrection = false;

        private double[] m_y = new double[3] {-1, -1, -1};
        private double[] m_x = new double[3] {-1, -1, -1};

        private double[] start_y = new double[2];
        private double[] start_x = new double[2];

        private double diff_x;
        private double diff_y;


        public ActiveDriftCorrection(int number_of_piezomirrors, double[] reference_position, ImageDrawer imgdrawer, PictureBox pb1, PictureBox pb2, PictureBox pbo)
        {
            m_refpos = reference_position;

            m_xyzstage = new smbStage(smbStage.StageType.PI_XYZNANOSTAGE);
            m_xyzstage.MoveToDist(29, 1);
            m_xyzstage.MoveToDist(29, 2);
            m_xyzstage.MoveToDist(9, 3);
            for (int i = 0; i < number_of_piezomirrors; i++) // Two piezomirrors
            {
                m_piezomirrors.Add(new smbStage(smbStage.StageType.PI_PIEZOMIRROR));
                m_piezomirrors[i].MoveToDist(1, 1);
                m_piezomirrors[i].MoveToDist(1, 2);
            }
            m_imgdrawer = imgdrawer;
            m_pbs[0] = pb1;
            m_pbs[1] = pb2;
            m_pbs[2] = pbo;
        }

        public void setXY(int n, int[] xy)
        {
            m_y[n] = xy[0];
            m_x[n] = xy[1];

            if (n < 2) {
                start_y[n] = xy[0];
                start_x[n] = xy[1];
            }
            else
            {
                diff_x = m_x[2] - start_x[0];
                diff_y = m_y[2] - start_y[0];
            }

            m_imgdrawer.ADCCursorXYs[n, 0] = xy[0];
            m_imgdrawer.ADCCursorXYs[n, 1] = xy[1];
        }

        private void ActiveDriftCorrectionThread()
        {
            int i, j, n;
            byte dumpval, max, min;

            double[] fitwindow = new double[49];
            int[][] displaywindows = new int[3][];
            double[] fitparams = new double[6];
            double[] fit_xye = new double[3];

            double dx, dy;

            for (n = 0; n < 3; n++) displaywindows[n] = new int[49];

            while (m_activedriftcorrection)
            {
                Thread.Sleep(10);

                for (n = 0; n < 3;  n++)
                {
                    max = 0;
                    min = 255;
                    if (m_imgdrawer.dump_array != null) m_imgdrawer.dumparray_sem.WaitOne();
                    for (i = 0; i < 7; i++)
                    {
                        for (j = 0; j < 7; j++)
                        {
                            dumpval = m_imgdrawer.dump_array[(int)m_y[n] - 3 + j, (int)m_x[n] - 3 + i];
                            if (dumpval > max) max = dumpval;
                            if (dumpval < min) min = dumpval;
                            fitwindow[j * 7 + i] = dumpval;
                        }
                    }
                    m_imgdrawer.dumparray_sem.Release();

                    fitparams[0] = min; fitparams[1] = max;
                    fitparams[2] = 1.5; fitparams[3] = 1.5;
                    fitparams[4] = 4.0; fitparams[5] = 4.0;
                    
                    fit_xye = gaussFit.fit(fitparams, fitwindow);
                    if (fit_xye[2] < 2000)
                    {
                        if (fit_xye[0] < 6 && fit_xye[0] > 1)
                            m_y[n] = ((int)m_y[n] - 3 + fit_xye[1]);
                        if (fit_xye[1] < 6 && fit_xye[1] > 1)
                            m_x[n] = ((int)m_x[n] - 3 + fit_xye[0]);
                    }
                    m_imgdrawer.ADCCursorXYs[n, 0] = (int)m_y[n];
                    m_imgdrawer.ADCCursorXYs[n, 1] = (int)m_x[n];

                    if (n < 2) // Pinhole
                    {
                        if (n == 0) n = 1; // Hardcode to exchange piezomirror order
                        else if (n == 1) n = 0;

                        m_piezoindex = n; // Set current Piezomirror
                        
                        if (m_y[n] > start_y[n]) moveDiffPiezomirror(-0.002, 1); // Move up
                        else if (m_y[n] < start_y[n]) moveDiffPiezomirror(-0.01, 1); // Move down

                        if (m_x[n] < start_x[n]) moveDiffPiezomirror(0.002, 2); // Move left
                        else if (m_x[n] > start_x[n]) moveDiffPiezomirror(-0.01, 2); // Move right
                    }
                    else // Object
                    {
                        dx = m_x[2] - m_x[0];
                        dy = m_y[2] - m_y[0];

                        if (dx > diff_x) moveDiffXYZstage(Math.Sign(dx) * 0.002, 1);
                        else if (dx < diff_x) moveDiffXYZstage(-Math.Sign(dx) * 0.002, 1);

                        if (dy > diff_y) moveDiffXYZstage(-Math.Sign(dy) * 0.002, 2);
                        else if (dy < diff_y) moveDiffXYZstage(Math.Sign(dy) * 0.002, 2);
                    }
                }

                if (m_imgdrawer.display_array != null) m_imgdrawer.displayarray_sem.WaitOne();
                for (i = 0; i < 7; i++)
                {
                    for (j = 0; j < 7; j++)
                    {
                        for (n = 0; n < 3; n++)
                        {
                            displaywindows[n][j * 7 + i] = m_imgdrawer.display_array[((int)m_y[n] - 3 + j) * 512 + (int)m_x[n] - 3 + i];
                        }
                    }
                }
                m_imgdrawer.displayarray_sem.Release();

                Bitmap[] bitmap = new Bitmap[3];
                unsafe
                {
                    for (n = 0; n < 3; n++)
                    {
                        fixed (int* intPtr = &displaywindows[n][0])
                        {
                            bitmap[n] = new Bitmap(7, 7, 4 * ((7 * 4 + 3) / 4), PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                        }
                    }
                }
                for (n = 0; n < 3; n++) m_pbs[n].Image = bitmap[n];
            }
        }

        public void moveDiffXYZstage(double distdiff, uint axis)
        {
            double dist;

            if (axis == 1)
            {
                dist = m_xyzstage.m_distx;
            }
            else if (axis == 2)
            {
                dist = m_xyzstage.m_disty;
            }
            else
            {
                dist = m_xyzstage.m_distz;
            }
            m_xyzstage.MoveToDist(dist + distdiff, axis);
        }

        public void moveDiffPiezomirror(double anglediff, uint axis)
        {
            double angle;

            if (axis == 1)
            {
                angle = m_piezomirrors[m_piezoindex].m_distx;
            }
            else
            {
                angle = m_piezomirrors[m_piezoindex].m_disty;
            }
            m_piezomirrors[m_piezoindex].MoveToDist(angle + anglediff, axis);
        }

        public double[] getCurrentPosXYZstage()
        {
            return new double[] { m_xyzstage.m_distx, m_xyzstage.m_disty, m_xyzstage.m_distz};
        }

        public double[] getCurrentAnglePiezomirror()
        {
            return new double[] {m_piezomirrors[m_piezoindex].m_distx, m_piezomirrors[m_piezoindex].m_disty};
        }

        public void setPiezomirrorIndex(int index)
        {
            m_piezoindex = index;
        }

        public void StartADC()
        {
            m_adcThread = new Thread(new ThreadStart(ActiveDriftCorrectionThread));
            m_adcThread.Priority = ThreadPriority.Normal;
            m_activedriftcorrection = true;
            m_adcThread.IsBackground = true;
            m_adcThread.Start();
        }

        public void StopADC()
        {
            m_activedriftcorrection = false;
        }

        ~ActiveDriftCorrection()
        {
            StopADC();
            if (m_adcThread != null) m_adcThread.Join();
        }
    }
}
