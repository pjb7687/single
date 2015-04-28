using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NIDAQmx;

namespace SMBdevices
{
    public class smbShutter
    {
        private List<int> m_lasertaskhandles = new List<int> { };
        private List<int> m_countertaskhandles = new List<int> { };
        private List<int> m_alexindex = new List<int> { };

        public enum ShutterType
        {
            NI_DAQ
        }
        private ShutterType m_shuttertype;

        public smbShutter(ShutterType shutter)
        {
            m_shuttertype = shutter;
        }

        ~smbShutter()
        {
            ClearALEX();
        }

        public void AddLaser(string lines)
        {
            switch (m_shuttertype)
            {
                case ShutterType.NI_DAQ:
                    int taskHandle = 0;
                    CounterBoard.DAQmxCreateTask("", ref taskHandle);
                    CounterBoard.DAQmxCreateDOChan(taskHandle, lines, "", CounterBoard.DAQmx_Val_ChanPerLine);
                    m_lasertaskhandles.Add(taskHandle);
                    break;
            }
        }

        public void AddCounterBoard(string counter, string countersrc, string triggersrc)
        {
            switch (m_shuttertype)
            {
                case ShutterType.NI_DAQ:
                    int taskHandle = 0;
                    CounterBoard.DAQmxCreateTask("", ref taskHandle);
                    CounterBoard.DAQmxCreateCOPulseChanTicks(taskHandle, counter, "alex_chan", countersrc, CounterBoard.DAQmx_Val_Low, 2, 2, 2);
                    CounterBoard.DAQmxCfgDigEdgeStartTrig(taskHandle, triggersrc, 0); // 0: Rising Edge
                    CounterBoard.DAQmxSetStartTrigRetriggerable(taskHandle, true);
                    m_countertaskhandles.Add(taskHandle);
                    break;
            }
        }

        public void AddALEX(int index)
        {
            m_alexindex.Add(index);
        }

        public void StartALEX(double exptime)
        {
            int initdelay = 0;
            int ExpTicks = (int)(exptime * 2e7);
            int TimeInterVening = 6400; // ccd intervening time 320 us
            int PreEnd = 28000 + 6400; // mechanical shutter closing time 1400us + 320us

            int taskHandle;
            for (int i = 0; i < m_alexindex.Count;  i++)
            {
                taskHandle = m_countertaskhandles[m_alexindex[i]];

                CounterBoard.DAQmxSetCOPulseHighTicks(taskHandle, "alex_chan", ExpTicks - PreEnd);
                CounterBoard.DAQmxSetCOPulseLowTicks(taskHandle, "alex_chan", ExpTicks * (m_alexindex.Count-1));
                CounterBoard.DAQmxSetCOPulseTicksInitialDelay(taskHandle, "alex_chan", initdelay);
                initdelay += ExpTicks + TimeInterVening - 300;

                CounterBoard.DAQmxStartTask(taskHandle);
            }
        }

        public void StopALEX()
        {
            for (int i = 0; i < m_alexindex.Count; i++)
                CounterBoard.DAQmxStopTask(m_countertaskhandles[m_alexindex[i]]);
        }

        public void ClearALEX()
        {
            int i;
            StopALEX();
            for (i=0; i < m_lasertaskhandles.Count; i++)
                CounterBoard.DAQmxClearTask(m_lasertaskhandles[i]);
            for (i = 0; i < m_countertaskhandles.Count; i++)
                CounterBoard.DAQmxClearTask(m_countertaskhandles[i]);
            m_alexindex.Clear();
        }

        public void LaserOn(int index)
        {
            ushort[] data = { 1 }; int written = 1;
            CounterBoard.DAQmxWriteDigitalLines(m_lasertaskhandles[index], 1, true, 0, false, data, ref written, IntPtr.Zero);
        }

        public void LaserOff(int index)
        {
            ushort[] data = { 0 }; int written = 1;
            CounterBoard.DAQmxWriteDigitalLines(m_lasertaskhandles[index], 1, true, 0, false, data, ref written, IntPtr.Zero);
        }
    }
}
