using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments;
using NationalInstruments.DAQmx;

namespace SMBdevices
{
    public class smbShutter
    {
        private List<DigitalSingleChannelWriter> m_writers = new List<DigitalSingleChannelWriter> { };
        private List<Task> m_countertasks = new List<Task> { };
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

        }

        public void AddLaser(string lines)
        {
            switch (m_shuttertype)
            {
                case ShutterType.NI_DAQ:
                    Task task = new Task();
                    task.DOChannels.CreateChannel(lines, "", ChannelLineGrouping.OneChannelForEachLine);
                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(task.Stream);
                    m_writers.Add(writer);
                    break;
            }
        }

        public void AddCounterBoard(string counter, string countersrc, string triggersrc)
        {
            switch (m_shuttertype)
            {
                case ShutterType.NI_DAQ:
                    Task task = new Task();
                    task.COChannels.CreatePulseChannelTicks(counter, "", countersrc, COPulseIdleState.Low, 2, 2, 2);
                    task.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
                    task.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
                    task.Triggers.StartTrigger.DigitalEdge.Source = triggersrc;
                    task.Triggers.StartTrigger.Retriggerable = true;
                    m_countertasks.Add(task);
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

            Task task;
            for (int i = 0; i < m_alexindex.Count;  i++)
            {
                task = m_countertasks[m_alexindex[i]];
                task.COChannels[0].PulseHighTicks = ExpTicks - PreEnd;
                task.COChannels[0].PulseLowTicks = ExpTicks * (m_alexindex.Count-1);
                task.COChannels[0].PulseTicksInitialDelay = initdelay;
                initdelay += ExpTicks + TimeInterVening - 300;
                task.Start();
            }
        }

        public void StopALEX()
        {
            for (int i = 0; i < m_alexindex.Count; i++)
                m_countertasks[m_alexindex[i]].Stop();
        }

        public void ClearALEX()
        {
            StopALEX();
            m_alexindex.Clear();
        }

        public void LaserOn(int index)
        {
            m_writers[index].WriteSingleSampleSingleLine(true, true);
        }

        public void LaserOff(int index)
        {
            m_writers[index].WriteSingleSampleSingleLine(true, false);
        }
    }
}
