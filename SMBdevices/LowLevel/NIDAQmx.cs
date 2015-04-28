using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NIDAQmx
{
    public class CounterBoard
    {
        private const string dllname = "nicaiu.dll";

        public static int DAQmx_Val_ChanPerLine = 0;
        public static int DAQmx_Val_Low = 10214;

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxCreateTask")]
        public static extern int DAQmxCreateTask (string taskName, ref int taskHandle);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxClearTask")]
        public static extern int DAQmxClearTask(int taskHandle);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxCreateDOChan")]
        public static extern int DAQmxCreateDOChan (int taskHandle, string lines, string nameToAssignToLines, int lineGrouping);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxCreateCOPulseChanTicks")]
        public static extern int DAQmxCreateCOPulseChanTicks(int taskHandle, string counter, string nameToAssignToChannel, string sourceTerminal, int idleState, int initialDelay, int lowTicks, int highTicks);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxCfgDigEdgeStartTrig")]
        public static extern int DAQmxCfgDigEdgeStartTrig(int taskHandle, string triggerSource, int triggerEdge);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxSetStartTrigRetriggerable")]
        public static extern int DAQmxSetStartTrigRetriggerable(int taskHandle, bool data);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxSetCOPulseHighTicks")]
        public static extern int DAQmxSetCOPulseHighTicks(int taskHandle, string channel, int data);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxSetCOPulseLowTicks")]
        public static extern int DAQmxSetCOPulseLowTicks(int taskHandle, string channel, int data);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxSetCOPulseTicksInitialDelay")]
        public static extern int DAQmxSetCOPulseTicksInitialDelay(int taskHandle, string channel, int data);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxStartTask")]
        public static extern int DAQmxStartTask(int taskHandle);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxStopTask")]
        public static extern int DAQmxStopTask(int taskHandle);

        [System.Runtime.InteropServices.DllImportAttribute(dllname, CallingConvention = CallingConvention.StdCall, EntryPoint = "DAQmxWriteDigitalLines")]
        public static extern int DAQmxWriteDigitalLines(int taskHandle, int numSampsPerChan, bool autoStart, double timeout, bool dataLayout, ushort[] writeArray, ref int sampsPerChanWritten, IntPtr reserved);
    }
}
