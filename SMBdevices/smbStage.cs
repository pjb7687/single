using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace SMBdevices
{
    public class smbStage
    {
        public enum StageType
        {
            MCL_CFOCUS,
            PI_ZSTAGE,
            PI_XYZNANOSTAGE
        }

        private StageType m_stage;

        private int m_handle;
        public double m_distx = 0;
        public double m_disty = 0;
        public double m_distz = 0;
        private SerialPort m_serialport;

        public smbStage(StageType stage)
        {
            m_stage = stage;
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    m_handle = Madlib.MCL_InitHandle();
                    if (m_handle == 0) throw new Exception();
                    double Cal = Madlib.MCL_GetCalibration(3, m_handle);
                    Madlib.MCL_SingleWriteN(Cal * .50, 3, m_handle);
                    m_distz = Cal * .50;
                    break;
                case StageType.PI_ZSTAGE:
                    string[] ports = SerialPort.GetPortNames();
                    foreach (string port in ports)
                    {
                        try
                        {
                            m_serialport = new SerialPort(port);

                            m_serialport.BaudRate = 9600;
                            m_serialport.DataBits = 8;
                            m_serialport.Parity = Parity.None;
                            m_serialport.StopBits = StopBits.One;
                            m_serialport.Handshake = Handshake.None;

                            m_serialport.Open();

                            m_serialport.Write("*IDN?\n");
                            Thread.Sleep(200);
                            if (m_serialport.ReadExisting().Substring(0, 4) == "E516")
                            {
                                // Found Device
                                m_serialport.Write("ONL 1\n");
                                Thread.Sleep(100);
                                m_serialport.Write("VCO A1\n");
                                Thread.Sleep(100);
                                m_serialport.Write("VEL A100\n");
                                Thread.Sleep(100);
                                m_serialport.Write("NLM A19\n");
                                Thread.Sleep(100);
                                m_serialport.Write("MOV A60.00\n");
                                m_distz = 60.0;
                                break;
                            }
                            else { m_serialport.Close(); };
                        }
                        catch
                        {
                            m_serialport.Close();
                        }
                    }
                    if (!m_serialport.IsOpen) throw new Exception();
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

        ~smbStage()
        {
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    Madlib.MCL_ReleaseAllHandles();
                    break;
                case StageType.PI_ZSTAGE:
                    m_serialport.Close();
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

        public void MoveToDist(double dist, uint axis)
        {
            // dist: in um
            // axis: 1 means x, 2 means y, and 3 means z
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    if (axis != 3) return;
                    Madlib.MCL_SingleWriteN(dist, axis, m_handle);
                    m_distz = dist;
                    break;
                case StageType.PI_ZSTAGE:
                    if (axis != 3) return;
                    m_serialport.Write(String.Format("MOV A{0:0.000}\n", dist));
                    m_distz = dist;
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    break;
            }
        }

    }
}
