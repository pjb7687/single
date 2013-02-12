using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SMBdevices
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct ProductInformation
    {

        /// unsigned char
        public byte axis_bitmap;

        /// short
        public short ADC_resolution;

        /// short
        public short DAC_resolution;

        /// short
        public short Product_id;

        /// short
        public short FirmwareVersion;

        /// short
        public short FirmwareProfile;
    }

    public class Madlib
    {

        /// MCL_SUCCESS -> 0
        public const int MCL_SUCCESS = 0;

        /// MCL_GENERAL_ERROR -> -1
        public const int MCL_GENERAL_ERROR = -1;

        /// MCL_DEV_ERROR -> -2
        public const int MCL_DEV_ERROR = -2;

        /// MCL_DEV_NOT_ATTACHED -> -3
        public const int MCL_DEV_NOT_ATTACHED = -3;

        /// MCL_USAGE_ERROR -> -4
        public const int MCL_USAGE_ERROR = -4;

        /// MCL_DEV_NOT_READY -> -5
        public const int MCL_DEV_NOT_READY = -5;

        /// MCL_ARGUMENT_ERROR -> -6
        public const int MCL_ARGUMENT_ERROR = -6;

        /// MCL_INVALID_AXIS -> -7
        public const int MCL_INVALID_AXIS = -7;

        /// MCL_INVALID_HANDLE -> -8
        public const int MCL_INVALID_HANDLE = -8;

        /// Return Type: void
        ///version: short*
        ///revision: short*
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_DLLVersion")]
        public static extern void MCL_DLLVersion(ref short version, ref short revision);


        /// Return Type: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_InitHandle")]
        public static extern int MCL_InitHandle();


        /// Return Type: int
        ///device: short
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GrabHandle")]
        public static extern int MCL_GrabHandle(short device);


        /// Return Type: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_InitHandleOrGetExisting")]
        public static extern int MCL_InitHandleOrGetExisting();


        /// Return Type: int
        ///device: short
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GrabHandleOrGetExisting")]
        public static extern int MCL_GrabHandleOrGetExisting(short device);


        /// Return Type: int
        ///serial: short
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetHandleBySerial")]
        public static extern int MCL_GetHandleBySerial(short serial);


        /// Return Type: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GrabAllHandles")]
        public static extern int MCL_GrabAllHandles();


        /// Return Type: int
        ///handles: int*
        ///size: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetAllHandles")]
        public static extern int MCL_GetAllHandles(ref int handles, int size);


        /// Return Type: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_NumberOfCurrentHandles")]
        public static extern int MCL_NumberOfCurrentHandles();


        /// Return Type: void
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ReleaseHandle")]
        public static extern void MCL_ReleaseHandle(int handle);


        /// Return Type: void
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ReleaseAllHandles")]
        public static extern void MCL_ReleaseAllHandles();


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_SingleReadZ")]
        public static extern double MCL_SingleReadZ(int handle);


        /// Return Type: double
        ///axis: unsigned int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_SingleReadN")]
        public static extern double MCL_SingleReadN(uint axis, int handle);


        /// Return Type: int
        ///position: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_SingleWriteZ")]
        public static extern int MCL_SingleWriteZ(double position, int handle);


        /// Return Type: int
        ///position: double
        ///axis: unsigned int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_SingleWriteN")]
        public static extern int MCL_SingleWriteN(double position, uint axis, int handle);


        /// Return Type: double
        ///position: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_MonitorZ")]
        public static extern double MCL_MonitorZ(double position, int handle);


        /// Return Type: double
        ///position: double
        ///axis: unsigned int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_MonitorN")]
        public static extern double MCL_MonitorN(double position, uint axis, int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ReadEncoderZ")]
        public static extern double MCL_ReadEncoderZ(int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ResetEncoderZ")]
        public static extern int MCL_ResetEncoderZ(int handle);


        /// Return Type: int
        ///milliradians: double
        ///actual: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ThetaX")]
        public static extern int MCL_ThetaX(double milliradians, ref double actual, int handle);


        /// Return Type: int
        ///milliradians: double
        ///actual: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ThetaY")]
        public static extern int MCL_ThetaY(double milliradians, ref double actual, int handle);


        /// Return Type: int
        ///position: double
        ///actual: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_MoveZCenter")]
        public static extern int MCL_MoveZCenter(double position, ref double actual, int handle);


        /// Return Type: int
        ///position: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_LevelZ")]
        public static extern int MCL_LevelZ(double position, int handle);


        /// Return Type: int
        ///focusModeOn: boolean
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_CFocusSetFocusMode")]
        public static extern int MCL_CFocusSetFocusMode([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)] bool focusModeOn, int handle);


        /// Return Type: int
        ///relativePositionChange: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_CFocusStep")]
        public static extern int MCL_CFocusStep(double relativePositionChange, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///milliseconds: double
        ///waveform: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ReadWaveFormN")]
        public static extern int MCL_ReadWaveFormN(uint axis, uint DataPoints, double milliseconds, ref double waveform, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///milliseconds: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_Setup_ReadWaveFormN")]
        public static extern int MCL_Setup_ReadWaveFormN(uint axis, uint DataPoints, double milliseconds, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///waveform: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_Trigger_ReadWaveFormN")]
        public static extern int MCL_Trigger_ReadWaveFormN(uint axis, uint DataPoints, ref double waveform, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///milliseconds: double
        ///waveform: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_LoadWaveFormN")]
        public static extern int MCL_LoadWaveFormN(uint axis, uint DataPoints, double milliseconds, ref double waveform, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///milliseconds: double
        ///waveform: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_Setup_LoadWaveFormN")]
        public static extern int MCL_Setup_LoadWaveFormN(uint axis, uint DataPoints, double milliseconds, ref double waveform, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_Trigger_LoadWaveFormN")]
        public static extern int MCL_Trigger_LoadWaveFormN(uint axis, int handle);


        /// Return Type: int
        ///axis: unsigned int
        ///DataPoints: unsigned int
        ///waveform: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_TriggerWaveformAcquisition")]
        public static extern int MCL_TriggerWaveformAcquisition(uint axis, uint DataPoints, ref double waveform, int handle);


        /// Return Type: int
        ///clock: int
        ///mode: int
        ///axis: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_IssBindClockToAxis")]
        public static extern int MCL_IssBindClockToAxis(int clock, int mode, int axis, int handle);


        /// Return Type: int
        ///clock: int
        ///mode: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_IssConfigurePolarity")]
        public static extern int MCL_IssConfigurePolarity(int clock, int mode, int handle);


        /// Return Type: int
        ///clock: int
        ///mode: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_IssSetClock")]
        public static extern int MCL_IssSetClock(int clock, int mode, int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_IssResetDefaults")]
        public static extern int MCL_IssResetDefaults(int handle);


        /// Return Type: int
        ///milliseconds: double
        ///clock: short
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_ChangeClock")]
        public static extern int MCL_ChangeClock(double milliseconds, short clock, int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_PixelClock")]
        public static extern int MCL_PixelClock(int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_LineClock")]
        public static extern int MCL_LineClock(int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_FrameClock")]
        public static extern int MCL_FrameClock(int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_AuxClock")]
        public static extern int MCL_AuxClock(int handle);


        /// Return Type: int
        ///adcfreq: double*
        ///dacfreq: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetClockFrequency")]
        public static extern int MCL_GetClockFrequency(ref double adcfreq, ref double dacfreq, int handle);


        /// Return Type: double
        ///axis: unsigned int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetCalibration")]
        public static extern double MCL_GetCalibration(uint axis, int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_TipTiltHeight")]
        public static extern double MCL_TipTiltHeight(int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_TipTiltWidth")]
        public static extern double MCL_TipTiltWidth(int handle);


        /// Return Type: int
        ///param0: double*
        ///param1: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_MinMaxThetaX")]
        public static extern int MCL_MinMaxThetaX(ref double param0, ref double param1, int handle);


        /// Return Type: int
        ///param0: double*
        ///param1: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_MinMaxThetaY")]
        public static extern int MCL_MinMaxThetaY(ref double param0, ref double param1, int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetTipTiltThetaX")]
        public static extern double MCL_GetTipTiltThetaX(int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetTipTiltThetaY")]
        public static extern double MCL_GetTipTiltThetaY(int handle);


        /// Return Type: double
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetTipTiltCenter")]
        public static extern double MCL_GetTipTiltCenter(int handle);


        /// Return Type: int
        ///param0: double*
        ///param1: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_CurrentMinMaxThetaX")]
        public static extern int MCL_CurrentMinMaxThetaX(ref double param0, ref double param1, int handle);


        /// Return Type: int
        ///param0: double*
        ///param1: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_CurrentMinMaxThetaY")]
        public static extern int MCL_CurrentMinMaxThetaY(ref double param0, ref double param1, int handle);


        /// Return Type: int
        ///param0: double*
        ///param1: double*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_CurrentMinMaxCenter")]
        public static extern int MCL_CurrentMinMaxCenter(ref double param0, ref double param1, int handle);


        /// Return Type: int
        ///version: short*
        ///profile: short*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetFirmwareVersion")]
        public static extern int MCL_GetFirmwareVersion(ref short version, ref short profile, int handle);


        /// Return Type: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetSerialNumber")]
        public static extern int MCL_GetSerialNumber(int handle);


        /// Return Type: int
        ///pi: ProductInformation*
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_GetProductInfo")]
        public static extern int MCL_GetProductInfo(ref ProductInformation pi, int handle);


        /// Return Type: void
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_PrintDeviceInfo")]
        public static extern void MCL_PrintDeviceInfo(int handle);


        /// Return Type: boolean
        ///milliseconds: int
        ///handle: int
        [System.Runtime.InteropServices.DllImportAttribute("Madlib.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "MCL_DeviceAttached")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool MCL_DeviceAttached(int milliseconds, int handle);

    }


}
