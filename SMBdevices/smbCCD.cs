using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMBdevices
{
    public class smbCCD : IDisposable
    {
        public enum CCDType
        {
            ANDOR_CCD,
            PROEM_CCD
        }

        public CCDType m_CCDType;
        public int m_CCDTemp;
        public bool m_gettingimage;
        public double m_exptime = 0.1;

        public int m_clipsize;
        public int m_imagewidth;
        public int m_imageheight;
        public int m_binsize;

        private uint m_Bufsize;

        private bool m_disposed = false;

        public smbCCD(CCDType ccd)
        {
            m_CCDType = ccd;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.Initialize("");
                    AndorCCD.GetTemperature(ref m_CCDTemp);
                    this.SetBinSize(1);

                    AndorCCD.SetExposureTime(0.1f);
                    AndorCCD.SetHSSpeed(0, 0);
                    AndorCCD.SetVSSpeed(3);
                    AndorCCD.SetAcquisitionMode(5);
                    AndorCCD.SetFrameTransferMode(1);
                    AndorCCD.SetTriggerMode(0);
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
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetEMCCDGain(gain);
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
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetTemperature(temp);
                    if (temp < -20)
                    {
                        AndorCCD.CoolerON();
                    }
                    else
                    {
                        AndorCCD.CoolerOFF();
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
                case CCDType.ANDOR_CCD:
                    AndorCCD.GetTemperature(ref m_CCDTemp);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }

            return m_CCDTemp;
        }

        public void SetBinSize(int bin_size)
        {
            m_binsize = bin_size;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetReadMode(4);
                    AndorCCD.SetImage(m_binsize, m_binsize, 1, 512, 1, 512);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
            m_Bufsize = (uint)((512 / m_binsize) * (512 / m_binsize));
            m_imagewidth = m_imageheight = 512 / m_binsize;
            m_clipsize = 30 / (512 / Math.Min(m_imagewidth, m_imageheight));
        }

        public void SetExpTime(double exptime)
        {
            m_exptime = exptime;
            AndorCCD.SetExposureTime((float)exptime);
        }

        public void GetImage(int[] imagebuf)
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    while (m_gettingimage)
                    {
                        if (AndorCCD.GetOldestImage(imagebuf, m_Bufsize) == AndorCCD.DRV_SUCCESS) break;
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
                case CCDType.ANDOR_CCD:
                    AndorCCD.PrepareAcquisition();
                    AndorCCD.SetShutter(1, 1, 0, 1);
                    AndorCCD.StartAcquisition();
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void ShutterOff()
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.AbortAcquisition();
                    AndorCCD.SetShutter(1, 2, 1, 1);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void Dispose()
        {
            if (m_disposed) return;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.AbortAcquisition();
                    AndorCCD.SetShutter(1, 2, 1, 1);
                    AndorCCD.SetTemperature(20);
                    AndorCCD.CoolerOFF();
                    AndorCCD.ShutDown();
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
            m_disposed = true;
        }

        ~smbCCD()
        {
            Dispose();
        }

    }
}
