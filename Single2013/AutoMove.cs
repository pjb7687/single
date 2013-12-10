using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SMBdevices;

namespace Single2013
{
    class AutoMove
    {
        private smbStage m_amstage;

        private bool m_automove;
        private ImageDrawer m_imgdrawer;
        private Thread m_automoveThread;
        private frmTIRF m_frm;

        private int m_count;
        private int[] m_dists;

        public AutoMove(ImageDrawer imgdrawer, frmTIRF frm)
        {
            m_imgdrawer = imgdrawer;
            m_frm = frm;
            m_amstage = new smbStage(smbStage.StageType.ASI_MS2000);
        }

        private void AutoMoveThread()
        {
            int i;
            for (i = 0; i < m_count; i++)
            {
                if (!m_automove) break;

                try
                {
                    m_frm.Invoke(new frmTIRF.startFilmingDelegate(m_frm.startFilming));
                }
                catch { }

                Thread.Sleep(500);

                while (m_imgdrawer.m_filming)
                {
                    Thread.Sleep(10);
                }

                string[] lines = { m_amstage.m_distx.ToString(), m_amstage.m_disty.ToString(), m_amstage.m_distz.ToString() };
                System.IO.File.WriteAllLines(m_frm.m_filename.Split('.')[0] + "_dists.txt", lines);

                m_amstage.MoveToDist(m_dists[0] + m_amstage.m_distx, 1);
                m_amstage.MoveToDist(m_dists[1] + m_amstage.m_disty, 2);
                m_amstage.MoveToDist(m_dists[2] + m_amstage.m_distz, 3);
                Thread.Sleep(1000);
            }
        }

        public void StartAutoMove(int count, int[] dists)
        {
            m_automove = true;
            m_automoveThread = new Thread(new ThreadStart(AutoMoveThread));
            m_automoveThread.Priority = ThreadPriority.Normal;
            m_automoveThread.IsBackground = true;
            m_automoveThread.Start();

            m_count = count;
            m_dists = dists;
        }

        public void StopAutoMove()
        {
            m_automove = false;
        }

        ~AutoMove()
        {
            m_automove = false;
            if (m_automoveThread != null)
                m_automoveThread.Join();
        }
    }
}
