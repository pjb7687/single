using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMBdevices
{
    public class smbStage
    {
        public enum StageType
        {
            MCL_CFOCUS,
            PI_ZSTAGE,
            PI_XYZNANOSTAGE
        }

        private StageType m_stage;

        private int m_handle;
        public double m_distx = 0;
        public double m_disty = 0;
        public double m_distz = 0;

        public smbStage(StageType stage)
        {
            m_stage = stage;
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    m_handle = Madlib.MCL_InitHandle();
                    if (m_handle == 0) throw new Exception();
                    double Cal = Madlib.MCL_GetCalibration(3, m_handle);
                    Madlib.MCL_SingleWriteN(Cal * .50, 3, m_handle);
                    m_distz = Cal * .50;
                    break;
                case StageType.PI_ZSTAGE:
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

        ~smbStage()
        {
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    Madlib.MCL_ReleaseAllHandles();
                    break;
                case StageType.PI_ZSTAGE:
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

        public void MoveToDist(double dist, uint axis)
        {
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    if (axis != 3) return;
                    Madlib.MCL_SingleWriteN(dist, axis, m_handle);
                    m_distz = dist;
                    break;
                case StageType.PI_ZSTAGE:
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

    }
}
