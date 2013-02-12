using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMBdevices
{
    public class smbPiezo
    {
        public enum PiezoType{
            MCL_CFOCUS,
            PI_PIEZO
        }

        private PiezoType m_piezo;

        private int m_handle;
        private uint m_axis;

        public double m_dist;

        public smbPiezo(PiezoType piezo)
        {
            m_piezo = piezo;
            switch (m_piezo)
            {
                case PiezoType.MCL_CFOCUS:
                    m_axis = 3; // Z
                    m_handle = Madlib.MCL_InitHandle();
		            double Cal = Madlib.MCL_GetCalibration(m_axis, m_handle);
		            Madlib.MCL_SingleWriteN(Cal * .50, m_axis, m_handle);
                    m_dist = Cal * .50;
                    break;
                case PiezoType.PI_PIEZO:
                    break;
            }
        }

        ~smbPiezo()
        {
            switch (m_piezo)
            {
                case PiezoType.MCL_CFOCUS:
                    Madlib.MCL_ReleaseAllHandles();
                    break;
                case PiezoType.PI_PIEZO:
                    break;
            }
        }

        public void MoveToDist(double dist)
        {
            switch (m_piezo)
            {
                case PiezoType.MCL_CFOCUS:
                    Madlib.MCL_SingleWriteN(dist, m_axis, m_handle);
                    m_dist = dist;
                    break;
                case PiezoType.PI_PIEZO:
                    break;
            }
        }
        
    }
}
