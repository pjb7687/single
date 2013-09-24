using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SMBdevices;

namespace Single2013
{
    class AutoFocusing
    {
        private smbStage m_stage;
        private smbCCD m_ccd;

        private int m_selectedchannel;

        private frmTIRF m_frm;
        private ImageDrawer m_imgdrawer;

        private double m_exptime;
        private int m_calibcnt = 10;
        private double m_calibdistdelta = 0.02;
        private double m_stdev;

        private double[] m_fitvals = new double[2];

        public bool m_focusing;
        public int m_ignoredarkframe = 0;

        private double m_frameintensity;

        private Thread m_focusingThread;

        private double CalcFOM()
        {
	        double I = 0;
            double Idiffx = 0, Idiffy = 0;
            double subx, suby;

            if (m_selectedchannel > m_frm.m_chanum) return 0;

            int xstart = (m_ccd.m_imagewidth / m_frm.m_chanum) * (m_selectedchannel - 1);
            int xend = (m_ccd.m_imagewidth / m_frm.m_chanum) * m_selectedchannel;
            int ystart = 0; int yend = m_ccd.m_imageheight;

            while (m_focusing && m_imgdrawer.AFFlag) Thread.Sleep(10);
            m_imgdrawer.AFFlag = true;

            if (m_imgdrawer.dump_array != null) m_imgdrawer.dumparray_sem.WaitOne();

            for (int x = m_ccd.m_clipsize + xstart; x < xend - m_ccd.m_clipsize; x++)
            {
                for (int y = m_ccd.m_clipsize + ystart; y < yend - m_ccd.m_clipsize; y++)
                {
                    I += (double)((m_imgdrawer.dump_array[x, y]) * (m_imgdrawer.dump_array[x, y]));
                    subx = (double)(m_imgdrawer.dump_array[x, y] - m_imgdrawer.dump_array[(x + 2), y]);
                    suby = (double)(m_imgdrawer.dump_array[x, y] - m_imgdrawer.dump_array[x, (y + 2)]);
                    Idiffx += subx*subx;
                    Idiffy += suby*suby;
                    m_frameintensity = I;
				}
			}
            m_imgdrawer.dumparray_sem.Release();

	        return Idiffy/I - Idiffx/I;
        }

        void LinearFit(double[] x, double[] y, int dataSize, double[] fit_vals)
        {
            double SUMx = 0;     //sum of x values
            double SUMy = 0;     //sum of y values
            double SUMxy = 0;    //sum of x * y
            double SUMxx = 0;    //sum of x^2
            double AVGy = 0;     //mean of y
            double AVGx = 0;     //mean of x

            //calculate various sums 
            for (int i = 0; i < dataSize; i++)
            {
                //sum of x
                SUMx = SUMx + x[i];
                //sum of y
                SUMy = SUMy + y[i];
                //sum of squared x*y
                SUMxy = SUMxy + x[i] * y[i];
                //sum of squared x
                SUMxx = SUMxx + x[i] * x[i];
            }

            //calculate the means of x and y
            AVGy = SUMy / dataSize;
            AVGx = SUMx / dataSize;

            //slope or a1
            fit_vals[1] = (dataSize * SUMxy - SUMx * SUMy) / (dataSize * SUMxx - SUMx * SUMx);

            //y intercept or a0
            fit_vals[0] = AVGy - fit_vals[1] * AVGx;
        }

        private void FocusingThread()
        {
            double fom, distdelta;
            List <double> lastintensities = new List<double> { };
            int ALEXcount = 0;
            bool isignore = false;
            int i;
            while (m_focusing)
            {
                if (m_ignoredarkframe != 0)
                {
                    lastintensities.Add(m_frameintensity);
                    if (ALEXcount == m_ignoredarkframe - 1)
                        lastintensities.RemoveAt(0);
                    else
                        ALEXcount++;
                }

                fom = CalcFOM();
                
                if (m_ignoredarkframe != 0)
                {
                    isignore = false;
                    foreach (double lastintensity in lastintensities)
                    {
                        if (lastintensity > m_frameintensity)
                        {
                            isignore = true;
                            break;
                        }
                    }
                    if (isignore) continue;
                }
                if (Math.Abs(fom) > 4 * m_stdev)
                {
                    distdelta = -fom * m_fitvals[1] / 2;

                    if (Math.Abs(distdelta) < 0.5) // for safety
                        m_stage.MoveToDist(m_stage.m_distz + distdelta, 3);
                }
                else if (fom < 0)
                {
                    m_stage.MoveToDist(m_stage.m_distz + 0.002, 3);
                }
                else
                {
                    m_stage.MoveToDist(m_stage.m_distz - 0.002, 3);
                }
                // Direct insertion of the values in a thread may cause a conflict between threads.
                // So we should use 'Invoke' method for the main thread to put the values to the controls...
                try
                {
                    m_frm.Invoke(new frmTIRF.updateAFInfoDelegate(m_frm.updateAFInfo), new object[] { fom, m_stage.m_distz });
                }
                catch { }
                
            }
        }

        public void StartFocusing()
        {
            m_focusingThread = new Thread(new ThreadStart(FocusingThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusing = true;
            m_focusingThread.IsBackground = true;
            m_focusingThread.Start();
        }

        public void StopFocusing()
        {
            m_focusing = false;
        }

        public void Calibrate(double exptime)
        {
            m_exptime = exptime;
            m_focusingThread = new Thread(new ThreadStart(CalibrateThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusingThread.Start();
        }

        public void FindingFocalPoint()
        {
            m_focusingThread = new Thread(new ThreadStart(FindingFocalPointThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusingThread.Start();
        }

        public void CalculateSTDEV()
        {
            m_focusingThread = new Thread(new ThreadStart(CalculateSTDEVThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusingThread.Start();
        }

        private void CalibrateThread()
        {
            int i, j;
            int N = 10;

            double[] foms = new double[m_calibcnt+1];
            double[] dists = new double[m_calibcnt+1];

            m_stage.MoveToDist(m_stage.m_distz - m_calibdistdelta * m_calibcnt / 2, 3);
            m_focusing = true;
            for (i = 0; i < m_calibcnt; i++)
            {
                Thread.Sleep(100);
                for (j = 0; j < N; j++)
                {
                    foms[i] += CalcFOM();
                    dists[i] += m_stage.m_distz;
                }
                foms[i] /= N;
                dists[i] /= N;
                m_stage.MoveToDist(m_stage.m_distz + m_calibdistdelta, 3);
            }
            foms[foms.Length-1] = CalcFOM();
            m_focusing = false;
            Thread.Sleep(100);
            dists[dists.Length - 1] = m_stage.m_distz;

            LinearFit(foms, dists, m_calibcnt, m_fitvals);

            try
            {
                m_frm.Invoke(new frmTIRF.CalibrateDoneDelegate(m_frm.CalibrateDone), new object[] { dists, foms, m_fitvals });
            }
            catch { }

        }

        private void CalculateSTDEVThread()
        {
            int i, stdevnum = (int)(10/m_exptime);
            double avg=0, avg2=0;
            double[] foms = new double[stdevnum];

            if (stdevnum < 10) stdevnum = 10;
            m_focusing = true;
            for (i = 0; i < stdevnum; i++)
            {
                foms[i] = CalcFOM();
                if (foms[i] < 0)
                    m_stage.MoveToDist(m_stage.m_distz + 0.002, 3);
                else
                    m_stage.MoveToDist(m_stage.m_distz - 0.002, 3);

                avg += foms[i]; avg2 += foms[i] * foms[i];
            }
            m_focusing = false;
            avg /= stdevnum; avg2 /= stdevnum;

            m_stdev = Math.Sqrt(Math.Abs(avg * avg - avg2));
            try
            {
                m_frm.Invoke(new frmTIRF.CalculateSTDEVDoneDelegate(m_frm.CalculateSTDEVDone), new object[] { foms, m_stdev });
            }
            catch { }
        }

        private void FindingFocalPointThread()
        {
            double fom, lastfom = 0;
            double distdelta;

            int pointnum =  (int)(5 / m_exptime);
            if (pointnum < 10) pointnum = 10;

            m_focusing = true;
            //m_stage.MoveToDist(m_stage.m_distz + m_calibdistdelta * m_calibcnt/2, 3);
            Thread.Sleep(50);
            fom = CalcFOM();
            distdelta = -fom * m_fitvals[1] / 2;

            if (Math.Abs(distdelta) < 0.5) // for safety
                m_stage.MoveToDist(m_stage.m_distz + distdelta, 3);

            for (int i = 0; i < pointnum; i++)
            {
                Thread.Sleep(50);
                fom = CalcFOM();
                if ((lastfom < 0 && fom > 0) || (lastfom > 0 && fom < 0)) break;
                lastfom = fom;
                distdelta = -fom * m_fitvals[1] / 2;

                if (Math.Abs(distdelta) < 0.5) // for safety
                    m_stage.MoveToDist(m_stage.m_distz + distdelta, 3);
            }
            m_focusing = false;
            try
            {
                m_frm.Invoke(new frmTIRF.FindingFocalPointDoneDelegate(m_frm.FindingFocalPointDone), new object[] { });
            }
            catch { }
        }

        public AutoFocusing(smbStage stage, smbCCD ccd, frmTIRF frm, ImageDrawer imgdrawer, int selectedchannel)
        {
            m_frm = frm;
            m_stage = stage;
            m_ccd = ccd;
            m_imgdrawer = imgdrawer;
            m_selectedchannel = selectedchannel;
        }

        ~AutoFocusing()
        {
            StopFocusing();
            if (m_focusingThread != null) m_focusingThread.Join();
        }
    }
}
