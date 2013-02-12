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
        private smbPiezo m_piezo;
        
        private int m_selectedchannel;

        private frmTIRF m_frm;

        public bool m_focusing;

        private Thread m_focusingThread;

        private double CalcFOM()
        {
	        double I = 0;
            double Idiffx = 0, Idiffy = 0;
            double subx, suby;

            int xstart = (CommonData.imagewidth / CommonData.chanum) * (m_selectedchannel - 1);
            int xend = (CommonData.imagewidth / CommonData.chanum) * m_selectedchannel;
            int ystart = 0; int yend = CommonData.imageheight;

            while (m_focusing && !ImageDrawer.isArraydumped) Thread.Sleep(10);
            ImageDrawer.isArraydumped = false;

            if (ImageDrawer.dump_array != null) ImageDrawer.dumparray_sem.WaitOne();

            for (int x = CommonData.clipsize + xstart; x < xend - CommonData.clipsize; x++)
            {
                for (int y = CommonData.clipsize + ystart; y < yend - CommonData.clipsize; y++)
                {
                    I += (double)((ImageDrawer.dump_array[x, y]) * (ImageDrawer.dump_array[x, y]));
                    subx = (double)(ImageDrawer.dump_array[x, y] - ImageDrawer.dump_array[(x + 2), y]);
                    suby = (double)(ImageDrawer.dump_array[x, y] - ImageDrawer.dump_array[x, (y + 2)]);
                    Idiffx += subx*subx;
                    Idiffy += suby*suby;
				}
			}
            ImageDrawer.dumparray_sem.Release();

	        return Idiffy/I - Idiffx/I;
        }

        private void FocusingThread()
        {
            double fom;
            while (m_focusing) {
                fom = CalcFOM();
                if (fom < 0)
                {
                    m_piezo.MoveToDist(m_piezo.m_dist + 0.002);
                }
                else
                {
                    m_piezo.MoveToDist(m_piezo.m_dist - 0.002);
                }
                // Direct insert of the values in a thread doesn't work.
                // Instead, we should use 'Invoke' method to put the values to the controls...
                m_frm.Invoke(new frmTIRF.updateAFInfoDelegate(m_frm.updateAFInfo), new object[] { "FOM: " + fom.ToString() + "  DIST: " + m_piezo.m_dist.ToString() });

            }
        }

        public void StartFocusing(int selectedchannel, frmTIRF frm)
        {
            m_selectedchannel = selectedchannel;
            m_focusingThread = new Thread(new ThreadStart(FocusingThread));
            m_focusingThread.Priority = ThreadPriority.BelowNormal;
            m_focusing = true;
            m_frm = frm;
            m_focusingThread.Start();
        }

        public void StopFocusing()
        {
            m_focusing = false;
        }

        public AutoFocusing(smbPiezo piezo)
        {
            m_piezo = piezo;
        }

        ~AutoFocusing()
        {
            StopFocusing();
            m_focusingThread.Join();
        }
    }
}
