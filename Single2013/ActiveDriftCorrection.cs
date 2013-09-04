using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMBdevices;

namespace Single2013
{
    class ActiveDriftCorrection
    {
        private smbStage m_xyzstage;
        private List<smbStage> m_piezomirrors = new List<smbStage> ();
        private double[] m_refpos;
        private int m_piezoindex;

        public ActiveDriftCorrection(int number_of_piezomirrors, double[] reference_position)
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
    }
}
