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
        private List<Tuple<string, string, string>> m_counterdata = new List<Tuple<string, string, string>> { };
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
            ClearLasers();
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
                    m_counterdata.Add(new Tuple<string, string, string>(counter, countersrc, triggersrc));
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

            int taskHandle = 0;
            for (int i = 0; i < m_alexindex.Count;  i++)
            {
                CounterBoard.DAQmxCreateTask("", ref taskHandle);
                CounterBoard.DAQmxCreateCOPulseChanTicks(taskHandle, m_counterdata[m_alexindex[i]].Item1, "", m_counterdata[m_alexindex[i]].Item2, CounterBoard.DAQmx_Val_Low, initdelay, ExpTicks * (m_alexindex.Count - 1), ExpTicks - PreEnd);
                CounterBoard.DAQmxCfgDigEdgeStartTrig(taskHandle, m_counterdata[m_alexindex[i]].Item3, 10280); // 10280: Rising Edge
                CounterBoard.DAQmxSetStartTrigRetriggerable(taskHandle, true);
                initdelay += ExpTicks + TimeInterVening - 300;
                CounterBoard.DAQmxStartTask(taskHandle);

                m_countertaskhandles.Add(taskHandle);
            }
        }

        public void StopALEX()
        {
            for (int i = 0; i < m_alexindex.Count; i++)
                CounterBoard.DAQmxStopTask(m_countertaskhandles[i]);
        }

        public void ClearALEX()
        {
            StopALEX();
            for (int i = 0; i < m_countertaskhandles.Count; i++)
                CounterBoard.DAQmxClearTask(m_countertaskhandles[i]);
            m_alexindex.Clear();
        }

        public void ClearLasers()
        {
            for (int i = 0; i < m_lasertaskhandles.Count; i++) { 
                CounterBoard.DAQmxStopTask(m_lasertaskhandles[i]);
                CounterBoard.DAQmxClearTask(m_lasertaskhandles[i]);
            }
            m_lasertaskhandles.Clear();
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
