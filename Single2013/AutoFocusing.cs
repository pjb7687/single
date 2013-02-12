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

        public bool m_focusing;

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
            m_imgdrawer.AFFlag = false;

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
				}
			}
            m_imgdrawer.dumparray_sem.Release();

	        return Idiffy/I - Idiffx/I;
        }

        private void FocusingThread()
        {
            double fom;
            while (m_focusing)
            {
                fom = CalcFOM();
                    if (fom < 0)
                        m_stage.MoveToDist(m_stage.m_distz + 0.002, 3);
                    else
                        m_stage.MoveToDist(m_stage.m_distz - 0.002, 3);
                // Direct insertion of the values in a thread may cause a conflict between threads.
                // So we should use 'Invoke' method for the main thread to put the values to the controls...
                m_frm.Invoke(new frmTIRF.updateAFInfoDelegate(m_frm.updateAFInfo), new object[] { "FOM: " + fom.ToString() + "  DIST: " + m_stage.m_distz.ToString() });
            }
        }

        public void StartFocusing(int selectedchannel, frmTIRF frm, ImageDrawer imgdrawer)
        {
            m_selectedchannel = selectedchannel;
            m_focusingThread = new Thread(new ThreadStart(FocusingThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusing = true;
            m_frm = frm;
            m_imgdrawer = imgdrawer;
            m_focusingThread.Start();
        }

        public void StopFocusing()
        {
            m_focusing = false;
        }

        public AutoFocusing(smbStage stage, smbCCD ccd)
        {
            m_stage = stage;
            m_ccd = ccd;
        }

        ~AutoFocusing()
        {
            StopFocusing();
            m_focusingThread.Join();
        }
    }
}
