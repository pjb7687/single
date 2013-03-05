using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMBdevices;
using System.IO.Ports;
using System.Threading;

namespace Single2013
{
    public class AutoFlow
    {
        private ImageDrawer m_imgdrawer;
        private smbCCD m_ccd;
        private frmTIRF m_frm;

        public bool m_autoflow = false;
        Thread m_autoflowThread;

        public List<smbPump> m_pumps = new List<smbPump>();

        private class FlowRule
        {
            public int m_pumpindex;
            public int m_framenum;
            public double m_diameter;
            public double m_volume;
            public double m_rate;
            public smbPump.runMode m_mode;
            public bool m_flowed = false;

            public FlowRule(int pumpindex, int framenum, double diameter, double volume, double rate, smbPump.runMode mode)
            {
                m_framenum = framenum; m_diameter = diameter; m_volume = volume; m_rate = rate; m_pumpindex = pumpindex; m_mode = mode;
            }
        }

        private List<FlowRule> rules = new List<FlowRule>();

        public AutoFlow(smbCCD ccd, ImageDrawer imgdrawer, frmTIRF frm)
        {
            m_imgdrawer = imgdrawer;
            m_ccd = ccd;
            m_frm = frm;

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                try
                {
                    m_pumps.Add(new smbPump(smbPump.pumpType.CHEMYX_FUSION, port));
                }
                catch
                {
                    try
                    {
                        m_pumps.Add(new smbPump(smbPump.pumpType.HARVARD_2000, port));
                    } catch {}
                }
            }

        }

        public void AddFlowRule(int pumpindex, int framenum, double diameter, double volume, double rate, smbPump.runMode mode)
        {
            rules.Add(new FlowRule(pumpindex, framenum, diameter, volume, rate, mode));
        }

        public void ClearRules()
        {
            rules.Clear();
        }

        public void InstantFlow(smbPump pump, double diameter, double volume, double rate, smbPump.runMode mode)
        {
            pump.RunWithSettings(diameter, smbPump.unitType.UL_PER_MIN, mode, volume, rate);
        }

        public void InstantFlow(int pumpindex, double diameter, double volume, double rate, smbPump.runMode mode)
        {
            m_pumps[pumpindex].RunWithSettings(diameter, smbPump.unitType.UL_PER_MIN, mode, volume, rate);
        }

        private void AutoFlowThread()
        {
            int fn, i;
            while (m_autoflow)
            {
                Thread.Sleep(10);
                if (rules.Count == 0 || !m_imgdrawer.m_filming) continue;
                fn = m_imgdrawer.m_framenum;
                for (i = 0; i < rules.Count; i++)
                {
                    if (fn > rules[i].m_framenum && rules[i].m_flowed == false)
                    {
                        InstantFlow(rules[i].m_pumpindex, rules[i].m_diameter, rules[i].m_volume, rules[i].m_rate, rules[i].m_mode);
                        rules[i].m_flowed = true;
                        try
                        {
                            m_frm.Invoke(new frmTIRF.updateAFLInfoDelegate(m_frm.updateAFLInfo), new object[] { i, rules[i].m_volume });
                        }
                        catch { }
                    }

                }
            }
        }

        public void StartAutoFlow()
        {
            m_autoflow = true;
            m_autoflowThread = new Thread(new ThreadStart(AutoFlowThread));
            m_autoflowThread.Priority = ThreadPriority.Normal;
            m_autoflowThread.IsBackground = true;
            m_autoflowThread.Start();
        }

        public void StopAutoFlow()
        {
            m_autoflow = false;
        }

        ~AutoFlow()
        {
            m_autoflow = false;
            if (m_autoflowThread != null)
                m_autoflowThread.Join();
        }

    }
}
