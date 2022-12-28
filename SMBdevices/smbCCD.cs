using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            ANDOR_SCMOS,
            PROEM_CCD
        }

        public CCDType m_CCDType;
        //public int m_CCDTemp;
        public double m_CCDTemp;
        public double m_exptime = 0.1;
        public int m_maxretrynum = 3;

        //추가함
        int CCDHandle = 0;


        public int m_clipsize;
        public int m_imagewidth;
        public int m_imageheight;
        public int m_binsize = 1;

        private bool m_isscmos = false;
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
                    ATMCD32CS.AndorSDK tmp1 = new ATMCD32CS.AndorSDK();
                    tmp1.GetAvailableCameras(ref cameranum);
                    break;
                case CCDType.ANDOR_SCMOS:
                    long tmpnum;
                    ATSDK3CS.DeviceWrapper tmp2 = new ATSDK3CS.DeviceWrapper();
                    tmp2.AT_GetInt(ATSDK3CS.DeviceWrapper.AT_HANDLE_SYSTEM, "Device Count", out tmpnum);
                    string sensortype;
                    int tmphandle;
                    for (int i = 0; i < tmpnum; i++)
                    {
                        tmp2.AT_Open(i, out tmphandle);
                        tmp2.AT_GetString(tmphandle, "GetSensorType", out sensortype);
                        tmp2.AT_Close(tmphandle);
                        if (sensortype != "SCMOS")
                        {
                            continue;
                        }
                        cameranum++;
                    }
                    break;
                case CCDType.PROEM_CCD:

                    break;
            }

            return cameranum;
        }

        //private ATMCD32CS.AndorSDK AndorCCD;
        private ATSDK3CS.DeviceWrapper AndorCCD3;

        public smbCCD(CCDType ccd, int CCDNum)

        {
            m_CCDType = ccd;
            //int CCDHandle = 0;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_SCMOS:
                    long tmpnum;
                    AndorCCD3 = new ATSDK3CS.DeviceWrapper();
                    AndorCCD3.AT_GetInt(ATSDK3CS.DeviceWrapper.AT_HANDLE_SYSTEM, "Device Count", out tmpnum);
                    string sensortype;
                    int tmphandle;
                    int cur_index = 0;
                    for (int i = 0; i < tmpnum; i++)
                    {
                        AndorCCD3.AT_Open(i, out tmphandle);
                        AndorCCD3.AT_GetString(tmphandle, "GetSensorType", out sensortype);
                        if (cur_index == CCDNum)
                        {
                            break;
                        }
                        AndorCCD3.AT_Close(tmphandle);
                        if (sensortype != "SCMOS")
                        {
                            continue;
                        }
                        cur_index++;
                    }
                    AndorCCD3.AT_InitialiseLibrary();

                    break;
                case CCDType.ANDOR_CCD:
                    float vsspeed = 0;
                    int vsspeed_index = 0;
                    string sensortype = "";
                    AndorCCD3 = new ATSDK3CS.DeviceWrapper();
                    AndorCCD3.AT_Open(CCDNum, out CCDHandle);
                    AndorCCD3.AT_GetString(CCDHandle, "SensorType", out sensortype);

                    //AndorCCD = new ATMCD32CS.AndorSDK();
                    AndorCCD3 = new ATSDK3CS.DeviceWrapper();
                    //AndorCCD.GetCameraHandle(CCDNum, ref CCDHandle);
                    //AndorCCD.SetCurrentCamera(CCDHandle);
                    AndorCCD3.AT_Open(CCDNum, out CCDHandle);
                    //AndorCCD.Initialize("");
                    AndorCCD3.AT_InitialiseLibrary();
                    //AndorCCD.GetTemperature(ref m_CCDTemp);
                    AndorCCD3.AT_GetFloat(CCDHandle, "SensorTemperature", out m_CCDTemp);
                    SetBinSize(1);
                    //AndorCCD.SetExposureTime(0.1f);
                    AndorCCD3.AT_SetFloat(CCDHandle, "ExposureTime", 0.1f);

                    //설정 사라짐
                    //AndorCCD.SetHSSpeed(0, 0);
                    //AndorCCD.GetFastestRecommendedVSSpeed(ref vsspeed_index, ref vsspeed);
                    //AndorCCD.SetVSSpeed(vsspeed_index);
                    //AndorCCD.SetAcquisitionMode(5); // Run till about
                    //AndorCCD.SetFrameTransferMode(1); // Frame transfer mode on: 1, off: 0
                    //AndorCCD.SetTriggerMode(0); // Internal trigger mode
                    AndorCCD3.AT_SetEnumIndex(CCDHandle, "TriggerMode", 0); // 0 : Internal,1 : Software, 2 : External, 3 : External Start, 4 : External Exposure
                    //AndorCCD.SetEMAdvanced(1); // Enable high gain
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

        public void SetTemp(long temp)
        {
            // Temp : Temperature of the CCD sensor
            if (temp > 20)
                temp = 20;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    //AndorCCD.SetTemperature(temp);
                    AndorCCD3.AT_SetFloat(CCDHandle, "TargetSensorTemperature", temp);

                    if (temp < -20)
                    {
                        //AndorCCD.CoolerON();
                        // sona에 없는 feature AndorCCD.AT_SetBool(CCDHandle, "SensorCooling", 1);
                        //AndorCCD.AT_SetEnumIndex(CCDHandle, L"SensorCooling", 1);
                    }
                    else
                    {
                        //AndorCCD.CoolerOFF();
                        // sona에 없는 feature AndorCCD.AT_SetBool(CCDHandle, "SensorCooling", 0);
                    }
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public double GetTemp()
        {
            // Temp : Temperature of the CCD sensor
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    //AndorCCD.GetTemperature(ref m_CCDTemp);
                    AndorCCD3.AT_GetFloat(CCDHandle, "SensorTemperature", out m_CCDTemp);
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
                    AndorCCD.SetReadMode(4); // AOILayout 으로 설정 가능할 듯!
                    //AndorCCD.SetImage(m_binsize, m_binsize, 1, m_rangewidth, 1, m_rangeheight);




                    //////수정중!!!!!!!!!!!!!
                    AndorCCD.AT_SetInt(CCDHandle, "AOIHBin", 1);
                    AndorCCD.AT_SetInt(CCDHandle, "AOIWidth", 1);
                    AndorCCD.AT_SetInt(CCDHandle, "AOILeft", 1);
                    AndorCCD.AT_SetInt(CCDHandle, "AOIVBin", 1);
                    AndorCCD.AT_SetInt(CCDHandle, "AOIHeight", 1);
                    AndorCCD.AT_SetInt(CCDHandle, "AOITop", 1);





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
            //AndorCCD.SetExposureTime((float)exptime);
            AndorCCD.AT_SetFloat(CCDHandle, "ExposureTime", (float)exptime);

        }

        public int GetImage(int[] imagebuf) // 무조건 필요
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
                    //todo : 로테이션 추가
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
                    //생략 가능, 지연시간 상승 AndorCCD.PrepareAcquisition();
                    //AndorCCD.SetShutter(1, 1, 1, 1);
                    //ShutterMode 외의 세부옵션 추가 안됐음
                    AndorCCD.AT_SetEnumIndex(CCDHandle, "ShutterMode", 1);
                    //AndorCCD.StartAcquisition();
                    AndorCCD.AT_Command(CCDHandle, "AcquisitionStart");

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
                    //AndorCCD.AbortAcquisition();
                    AndorCCD.AT_Command(CCDHandle, "AcquisitionStart");
                    // AndorCCD.SetShutter(1, 2, 1, 1);
                    //ShutterMode 외의 세부옵션 추가 안됐음
                    AndorCCD.AT_SetEnumIndex(CCDHandle, "ShutterMode", 2);
                    break;
                case CCDType.PROEM_CCD:
                    break;
            }
        }

        public void SetRotation(int rotation) //사라진 기능, 따로 코드를 짜야할 것 같음.
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
                    //AndorCCD.AbortAcquisition();
                    AndorCCD.AT_Command(CCDHandle, "AcquisitionStop");
                    //AndorCCD.SetShutter(1, 2, 1, 1);
                    //ShutterMode 외의 세부옵션 추가 안됐음
                    AndorCCD.AT_SetEnumIndex(CCDHandle, "ShutterMode", 2);
                    //AndorCCD.SetTemperature(20);
                    AndorCCD.AT_SetFloat(CCDHandle, "TargetSensorTemperature", 20);
                    //AndorCCD.CoolerOFF();
                    //AndorCCD.AT_SetBool(CCDHandle, "SensorCooling",0);
                    //AndorCCD.ShutDown();
                    AndorCCD.AT_Close(CCDHandle);
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
