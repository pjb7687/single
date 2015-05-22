using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        public double m_exptime = 0.1;
        public int m_maxretrynum = 3;

        public int m_clipsize;
        public int m_imagewidth;
        public int m_imageheight;
        public int m_binsize = 1;

        private int m_rangeheight = 1024;
        private int m_rangewidth = 1024;

        private uint m_Bufsize;

        private bool m_disposed = false;

        public static int getCameraNumber(CCDType ccd)
        {
            int cameranum = 0;
            switch (ccd)
            {
                case CCDType.ANDOR_CCD:
                    ATMCD32CS.AndorSDK tmp = new ATMCD32CS.AndorSDK();
                    tmp.GetAvailableCameras(ref cameranum);
                    break;
                case CCDType.PROEM_CCD:

                    break;
            }

            return cameranum;
        }

        private ATMCD32CS.AndorSDK AndorCCD;

        public smbCCD(CCDType ccd, int CCDNum)
        {
            m_CCDType = ccd;
            int CCDHandle = 0;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    float vsspeed = 0;
                    int vsspeed_index = 0;
                    AndorCCD = new ATMCD32CS.AndorSDK();
                    AndorCCD.GetCameraHandle(CCDNum, ref CCDHandle);
                    AndorCCD.SetCurrentCamera(CCDHandle);
                    AndorCCD.Initialize("");
                    AndorCCD.GetTemperature(ref m_CCDTemp);
                    this.SetBinSize(1);

                    AndorCCD.SetExposureTime(0.1f); 
                    AndorCCD.SetHSSpeed(0, 0);
                    AndorCCD.GetFastestRecommendedVSSpeed(ref vsspeed_index, ref vsspeed);
                    AndorCCD.SetVSSpeed(vsspeed_index);
                    AndorCCD.SetAcquisitionMode(5); // Run till about
                    AndorCCD.SetFrameTransferMode(1); // Frame transfer mode on: 1, off: 0
                    AndorCCD.SetTriggerMode(0); // Internal trigger mode
                    break;
                case CCDType.PROEM_CCD:
                    m_CCDTemp = 0;
                    break;
            }
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

        private void SetImageSize()
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetReadMode(4);
                    AndorCCD.SetImage(m_binsize, m_binsize, 1, m_rangewidth, 1, m_rangeheight);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
            m_Bufsize = (uint)((m_rangewidth / m_binsize) * (m_rangeheight / m_binsize));
            m_imagewidth = m_rangewidth / m_binsize;
            m_imageheight = m_rangeheight / m_binsize;
            m_clipsize = (int)(30.0 / (512.0 / Math.Min(m_imagewidth, m_imageheight)));
        }

        public float GetExptime()
        {
            float exptime = 0;
            float acc = 0;
            float kin = 0;

            AndorCCD.GetAcquisitionTimings(ref exptime, ref acc, ref kin);

            return exptime;
        }

        public void SetBinSize(int bin_size)
        {
            m_binsize = bin_size;
        }

        public void SetRange(int rangeWidth, int rangeHeight)
        {
            m_rangewidth = rangeWidth;
            m_rangeheight = rangeHeight;
            SetImageSize();
        }

        public void SetExpTime(double exptime)
        {
            m_exptime = exptime;
            AndorCCD.SetExposureTime((float)exptime);
        }

        public int GetImage(int[] imagebuf)
        {
            int retrynum = 0;
            uint err;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    err = AndorCCD.GetOldestImage(imagebuf, m_Bufsize);

                    if (err == ATMCD32CS.AndorSDK.DRV_SUCCESS) return 0;
                    else if (err == ATMCD32CS.AndorSDK.DRV_NO_NEW_DATA) return 2;
                    else retrynum++;

                    if (retrynum > m_maxretrynum)
                        return 1;

                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
            return 0;
        }

        public void ShutterOn()
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.PrepareAcquisition();
                    AndorCCD.SetShutter(1, 1, 1, 1);
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

        public void SetRotation(int rotation)
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    switch (rotation)
                    {
                        case 0:
                            AndorCCD.SetImageFlip(0, 0);
                            AndorCCD.SetImageRotate(0);
                            break;
                        case 1:
                            AndorCCD.SetImageFlip(0, 0);
                            AndorCCD.SetImageRotate(1);
                            break;
                        case 2:
                            AndorCCD.SetImageFlip(1, 1);
                            AndorCCD.SetImageRotate(0);
                            break;
                        case 3:
                            AndorCCD.SetImageFlip(0, 0);
                            AndorCCD.SetImageRotate(2);
                            break;
                    }
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
