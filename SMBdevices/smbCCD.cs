using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMBdevices
{
    public class smbCCD
    {
        public enum CCDType
        {
            ENDOR_CCD,
            PROEM_CCD
        }

        public CCDType m_CCDType;
        public int m_CCDTemp;
        public bool m_gettingimage;
        public double m_exptime = 0.1;
        
        private uint m_Bufsize;

        public smbCCD(CCDType ccd)
        {
            m_CCDType = ccd;
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.Initialize("");
                    EndorCCD.GetTemperature(ref m_CCDTemp);
                    this.SetBinSize(1, 1);

                    EndorCCD.SetExposureTime(0.1f);
                    EndorCCD.SetHSSpeed(0, 0);
                    EndorCCD.SetVSSpeed(3);
                    EndorCCD.SetAcquisitionMode(5);
                    EndorCCD.SetFrameTransferMode(1);
                    EndorCCD.SetTriggerMode(0);
                    break;
                case CCDType.PROEM_CCD:
                    m_CCDTemp = 0;
                    break;
            }
            m_gettingimage = false;
        }

        public void SetGain(int gain)
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.SetEMCCDGain(gain);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void SetTemp(int temp)
        {
            // Temp : Temperature of the CCD sensor
            if (temp > 20)
                temp = 20;
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.SetTemperature(temp);
                    if (temp < -20)
                    {
                        EndorCCD.CoolerON();
                    }
                    else
                    {
                        EndorCCD.CoolerOFF();
                    }
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public int GetTemp()
        {
            // Temp : Temperature of the CCD sensor
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.GetTemperature(ref m_CCDTemp);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }

            return m_CCDTemp;
        }

        public void SetBinSize(int bin_width, int bin_height)
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.SetReadMode(4);
                    EndorCCD.SetImage(bin_width, bin_height, 1, 512, 1, 512);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
            m_Bufsize = (uint)((512 / bin_height) * (512 / bin_width));
        }

        public void SetExpTime(double exptime)
        {
            m_exptime = exptime;
            EndorCCD.SetExposureTime((float)exptime);
        }

        public void GetImage(int[] imagebuf)
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    while (m_gettingimage)
                    {
                        if (EndorCCD.GetOldestImage(imagebuf, m_Bufsize) == EndorCCD.DRV_SUCCESS) break;
                    }
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void ShutterOn()
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.PrepareAcquisition();
                    EndorCCD.SetShutter(1, 1, 0, 1);
                    EndorCCD.StartAcquisition();
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void ShutterOff()
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.AbortAcquisition();
                    EndorCCD.SetShutter(1, 2, 1, 1);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        ~smbCCD()
        {
            switch (m_CCDType)
            {
                case CCDType.ENDOR_CCD:
                    EndorCCD.AbortAcquisition();
                    EndorCCD.SetShutter(1, 2, 1, 1);
                    EndorCCD.CoolerOFF();
                    EndorCCD.SetTemperature(20);
                    EndorCCD.ShutDown();
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

    }
}
