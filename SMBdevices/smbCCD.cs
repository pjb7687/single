using ATMCD32CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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
        public int m_CCDTemp;
        public double m_SCMOSTemp;
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

        // private uint m_Bufsize;
        private int m_Bufsize;

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


        private ATMCD32CS.AndorSDK AndorCCD;
        private ATSDK3CS.DeviceWrapper AndorSCMOS;

        public smbCCD(CCDType ccd, int CCDNum)

        {
            m_CCDType = ccd;
            string sensortype;

            switch (m_CCDType)
            {
                case CCDType.ANDOR_SCMOS:

                    m_rangeheight = 2048;
                    m_rangewidth = 2048;

                    AndorSCMOS = new ATSDK3CS.DeviceWrapper();
                    AndorSCMOS.AT_Open(CCDNum, out CCDHandle);
                    AndorSCMOS.AT_GetString(CCDHandle, "SensorType", out sensortype);
                    AndorSCMOS = new ATSDK3CS.DeviceWrapper();
                    AndorSCMOS.AT_InitialiseLibrary();
                    AndorSCMOS.AT_Open(CCDNum, out CCDHandle);
                    AndorSCMOS.AT_GetFloat(CCDHandle, "SensorTemperature", out m_SCMOSTemp); // OK...
                    SetBinSize(1);
                    AndorSCMOS.AT_SetFloat(CCDHandle, "ExposureTime", 0.1f); // OK..
                    //AndorSCMOS.AT_SetInt(CCDHandle, " AcquisitionMode", 5); 이런거 없음..
                    AndorSCMOS.AT_SetEnumIndex(CCDHandle, "TriggerMode", 0); // 0 : Internal,1 : Software, 2 : External, 3 : External Start, 4 : External Exposure

                    /* 공사 중 */

                    //long tmpnum;
                    //AndorSCMOS = new ATSDK3CS.DeviceWrapper();
                    //AndorSCMOS.AT_GetInt(ATSDK3CS.DeviceWrapper.AT_HANDLE_SYSTEM, "Device Count", out tmpnum);

                    //m_rangeheight = 2048;
                    //m_rangewidth = 2048;

                    //int tmphandle;
                    //int cur_index = 0;
                    //for (int i = 0; i < tmpnum; i++)
                    //{
                    //    AndorSCMOS.AT_Open(i, out tmphandle);
                    //    AndorSCMOS.AT_GetString(tmphandle, "GetSensorType", out sensortype);
                    //    if (cur_index == CCDNum)
                    //    {
                    //        break;
                    //    }
                    //    AndorSCMOS.AT_Close(tmphandle);
                    //    if (sensortype != "SCMOS")
                    //    {
                    //        continue;
                    //    }
                    //    cur_index++;
                    //}

                    break;

                case CCDType.ANDOR_CCD:
                    float vsspeed = 0;
                    int vsspeed_index = 0;

                    AndorCCD = new ATMCD32CS.AndorSDK();
                    AndorCCD.GetCameraHandle(CCDNum, ref CCDHandle);
                    AndorCCD.SetCurrentCamera(CCDHandle);
                    AndorCCD.Initialize("");

                    //SDK2 - 3 자료형이 안맞아서 수정해줘야함 (e.g. m_CCDTemp)
                    //AndorCCD.GetTemperature(ref m_CCDTemp);
                    SetBinSize(1);
                    AndorCCD.SetExposureTime(0.1f);
                    AndorCCD.SetHSSpeed(0, 0);
                    AndorCCD.GetFastestRecommendedVSSpeed(ref vsspeed_index, ref vsspeed);
                    AndorCCD.SetVSSpeed(vsspeed_index);
                    AndorCCD.SetAcquisitionMode(5); // Run till about
                    AndorCCD.SetFrameTransferMode(1); // Frame transfer mode on: 1, off: 0
                    AndorCCD.SetTriggerMode(0); // Internal trigger mode
                    AndorCCD.SetEMAdvanced(1); // Enable high gain
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
                case CCDType.ANDOR_SCMOS:
                    break;//SONA에는 gain이 필요 없을듯
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
                    AndorCCD.SetTemperature((int)temp);

                    if (temp < -20)
                    {
                        AndorCCD.CoolerON();
                    }
                    else
                    {
                        AndorCCD.CoolerOFF();
                    }

                    break;

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_SetFloat(CCDHandle, "TargetSensorTemperature", temp); //OK..

                    if (temp < -20)
                    {
                        AndorSCMOS.AT_SetBool(CCDHandle, "SensorCooling", 1); //OK..
                    }
                    else
                    {
                        AndorSCMOS.AT_SetBool(CCDHandle, "SensorCooling", 0); //OK..
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
                    AndorCCD.GetTemperature(ref m_CCDTemp);
                    return m_CCDTemp;

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_GetFloat(CCDHandle, "SensorTemperature", out m_SCMOSTemp); //OK..
                    return m_SCMOSTemp;

                case CCDType.PROEM_CCD:
                    break;
            }

            return 1;
        }

        private void SetImageSize()
        {
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetReadMode(4); // AOILayout 으로 설정 가능할 듯!
                    AndorCCD.SetImage(m_binsize, m_binsize, 1, m_rangewidth, 1, m_rangeheight);
                    break;

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_SetEnumIndex(CCDHandle, "AOILayout", 0);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOIHBin", m_binsize);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOIVBin", m_binsize);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOIWidth", m_rangewidth);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOILeft", 1);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOIHeight", m_rangeheight);
                    AndorSCMOS.AT_SetInt(CCDHandle, "AOITop", 1);
                    break;

                case CCDType.PROEM_CCD:
                    break;
            }

            // m_Bufsize를 unit으로 바꿔주는 코드 제거
            m_Bufsize = (m_rangewidth / m_binsize) * (m_rangeheight / m_binsize);
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

            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    AndorCCD.SetExposureTime((float)exptime);
                    break;

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_SetFloat(CCDHandle, "ExposureTime", (float)exptime);
                    break;

                case CCDType.PROEM_CCD:
                    break;

            }
        }

        public int GetImage(int[] imagebuf) // 무조건 필요
        {
            int retrynum = 0;
            uint err;
            switch (m_CCDType)
            {
                case CCDType.ANDOR_CCD:
                    err = AndorCCD.GetOldestImage(imagebuf, (uint)m_Bufsize);

                    if (err == ATMCD32CS.AndorSDK.DRV_SUCCESS) return 0;
                    else if (err == ATMCD32CS.AndorSDK.DRV_NO_NEW_DATA) return 2;
                    else retrynum++;

                    if (retrynum > m_maxretrynum)
                        return 1;

                    break;

                case CCDType.ANDOR_SCMOS:

                    IntPtr scmosbuf = Marshal.AllocHGlobal(m_Bufsize);
                    AndorSCMOS.AT_Command(CCDHandle, "AcquisitionStart");
                    AndorSCMOS.AT_QueueBuffer(CCDHandle, scmosbuf, m_Bufsize);
                    err = (uint)AndorSCMOS.AT_WaitBuffer(CCDHandle, out scmosbuf, out m_Bufsize, 1000);
                    AndorSCMOS.AT_Command(CCDHandle, "AcquisitionStop");
                    AndorSCMOS.AT_Flush(CCDHandle);
                    Marshal.Copy(scmosbuf, imagebuf, 0, imagebuf.Length);


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

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_SetEnumIndex(CCDHandle, "ShutterMode", 1);
                    AndorSCMOS.AT_Command(CCDHandle, "AcquisitionStart"); //ShutterMode 외의 세부옵션 추가 안됐음
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

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_Command(CCDHandle, "AcquisitionStop");
                    AndorSCMOS.AT_SetEnumIndex(CCDHandle, "ShutterMode", 2); //ShutterMode 외의 세부옵션 추가 안됐음
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
                case CCDType.ANDOR_SCMOS:
                    switch (rotation)
                    {
                        case 0:
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipX", 0);
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipY", 0);
                            //AndorCCD3.AT_SetInt(CCDHandle, "Rotation", 0);
                            break;
                        case 1:
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipX", 0);
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipY", 0);
                            //AndorCCD3.AT_SetInt(CCDHandle, "Rotation", 1);
                            break;
                        case 2:
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipX", 1);
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipY", 0);
                            //AndorCCD3.AT_SetInt(CCDHandle, "Rotation", 0);
                            break;
                        case 3:
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipX", 0);
                            //AndorCCD3.AT_SetBool(CCDHandle, "FlipY", 0);
                            //AndorCCD3.AT_SetInt(CCDHandle, "Rotation", 3);
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

                case CCDType.ANDOR_SCMOS:
                    AndorSCMOS.AT_Command(CCDHandle, "AcquisitionStop");
                    AndorSCMOS.AT_SetEnumIndex(CCDHandle, "ShutterMode", 2);//ShutterMode 외의 세부옵션 추가 안됐음
                    AndorSCMOS.AT_SetFloat(CCDHandle, "TargetSensorTemperature", 20);
                    AndorSCMOS.AT_SetBool(CCDHandle, "SensorCooling", 0);
                    AndorSCMOS.AT_Close(CCDHandle);
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
