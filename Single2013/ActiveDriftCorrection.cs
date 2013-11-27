using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMBdevices;
using System.Threading;

namespace Single2013
{
    class ActiveDriftCorrection
    {
        private List<smbStage> m_piezomirrors = new List<smbStage> ();
        private smbStage m_xyzstage;
        private ImageDrawer m_imgdrawer;
        private Thread m_adcThread;

        private double[] m_refpos;

        private int m_piezoindex;
        public bool m_activedriftcorrection = false;

        private double[] m_x = new double[3] {-1, -1, -1};
        private double[] m_y = new double[3] {-1, -1, -1};

        private double diff_x;
        private double diff_y;


        public ActiveDriftCorrection(int number_of_piezomirrors, double[] reference_position, ImageDrawer imgdrawer)
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
        }

        public void setXY(int n, int[] xy)
        {
            m_x[n] = xy[0];
            m_y[n] = xy[1];

            if (n == 2)
            {
                diff_x = m_x[0] - m_x[2];
                diff_y = m_y[0] - m_y[2];
            }

            m_imgdrawer.ADCCursorXYs[n, 0] = xy[0];
            m_imgdrawer.ADCCursorXYs[n, 1] = xy[1];
        }

        private void ActiveDriftCorrectionThread()
        {
            int i, j, n;
            byte dumpval, max, min;

            double[] fitwindow = new double[49];
            double[] fitparams = new double[6];
            double[] fit_xy;

            while (m_activedriftcorrection)
            {
                Thread.Sleep(10);
                if (m_imgdrawer.dump_array != null) m_imgdrawer.dumparray_sem.WaitOne();
                for (n = 0; n < 3;  n++)
                {
                    max = 0;
                    min = 255;
                    for (i = 0; i < 7; i++)
                    {
                        for (j = 0; j < 7; j++)
                        {
                            dumpval = m_imgdrawer.dump_array[(int)m_x[n] - 3 + i, (int)m_y[n] - 3 + j];
                            if (dumpval > max) max = dumpval;
                            if (dumpval < min) min = dumpval;
                            fitwindow[i * 7 + j] = dumpval;
                        }
                    }

                    fitparams[0] = min; fitparams[1] = max;
                    fitparams[2] = 1.5; fitparams[3] = 1.5;
                    fitparams[4] = 4.0; fitparams[5] = 4.0;

                    fit_xy = gaussFit.fit(fitparams, fitwindow);

                    m_x[n] = ((int)m_x[n] - 3 + fit_xy[0]);
                    m_y[n] = ((int)m_y[n] - 3 + fit_xy[1]);

                    if (n < 2) // Pinhole
                    {
                        m_piezoindex = n; // Set current Piezomirror

                        if (fit_xy[0] > 3.0)
                        {
                            moveDiffPiezomirror(0.05, 2);
                        }
                        else if (fit_xy[0] < 3.0)
                        {
                            moveDiffPiezomirror(-0.05, 2);
                        }
                        if (fit_xy[1] > 3.0)
                        {
                            moveDiffPiezomirror(-0.05, 1);
                        }
                        else if (fit_xy[1] < 3.0)
                        {
                            moveDiffPiezomirror(0.05, 1);
                        }
                    }
                    else // Object
                    {
                        if (m_x[0] - m_x[2] < diff_x)
                        {
                            moveDiffXYZstage(0.05, 1);
                        }
                        else if (m_x[0] - m_x[2] > diff_x)
                        {
                            moveDiffXYZstage(-0.05, 1);
                        }
                        if (m_y[0] - m_y[2] < diff_y)
                        {
                            moveDiffXYZstage(-0.05, 2);
                        }
                        else if (m_y[0] - m_y[2] > diff_y)
                        {
                            moveDiffXYZstage(0.05, 2);
                        }
                    }
                }
                m_imgdrawer.dumparray_sem.Release();
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
            m_adcThread.Priority = ThreadPriority.BelowNormal;
            m_activedriftcorrection = true;
            m_adcThread.IsBackground = true;
            m_adcThread.Start();
        }

        public void StopADC()
        {
            m_activedriftcorrection = false;
        }
    }
}
