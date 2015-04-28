using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using PI;
using Madlib;

namespace SMBdevices
{
    public class smbStage
    {
        public enum StageType
        {
            MCL_CFOCUS,
            PI_ZSTAGE,
            PI_XYZNANOSTAGE,
            PI_PIEZOMIRROR,
            ASI_MS2000
        }

        private StageType m_stage;

        private int m_handle;
        private int m_iControllerId;

        public double m_distx = 0;
        public double m_disty = 0;
        public double m_distz = 0;
        private SerialPort m_serialport;

        public smbStage(StageType stage)
        {
            string[] ports;

            m_stage = stage;
            switch (m_stage)
            {
                case StageType.ASI_MS2000:
                    ports = SerialPort.GetPortNames();
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

                            m_serialport.Write("N\r");
                            Thread.Sleep(200);
                            if (m_serialport.ReadExisting().Substring(7, 6) == "MS2000")
                            {
                                // Found Device
                                m_serialport.Write("W X Y Z\r");
                                Thread.Sleep(100);
                                string[] dists = m_serialport.ReadExisting().Split(' ');
                                m_distx = Convert.ToDouble(dists[1]);
                                m_disty = Convert.ToDouble(dists[2]);
                                m_distz = Convert.ToDouble(dists[3]);
                                break;
                            }
                            else { m_serialport.Close(); };
                        }
                        catch
                        {
                            m_serialport.Close();
                        }
                    }
                    break;
                case StageType.MCL_CFOCUS:
                    m_handle = CFocus.MCL_InitHandle();
                    if (m_handle == 0) throw new Exception();
                    double Cal = CFocus.MCL_GetCalibration(3, m_handle);
                    CFocus.MCL_SingleWriteN(Cal * .50, 3, m_handle);
                    m_distz = Cal * .50;
                    break;
                case StageType.PI_ZSTAGE:
                    ports = SerialPort.GetPortNames();
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
                    StringBuilder sUsbController = new StringBuilder(1024);

                    GCS2.EnumerateUSB(sUsbController, 1024, "PI E-712");
                    m_iControllerId = GCS2.ConnectUSB(sUsbController.ToString());

                    if (m_iControllerId < 0) throw new Exception();

                    GCS2.ONL(m_iControllerId, new int [] {1, 2, 3}, new int[] {1, 1, 1}, 3);

                    GCS2.SVO(m_iControllerId, "1 2 3", new int[] { 1, 1, 1 });
                    GCS2.MOV(m_iControllerId, "1 2 3", new double[] { 0, 0, 0 });
                    break;

                case StageType.PI_PIEZOMIRROR:
                    ports = SerialPort.GetPortNames();
                    foreach (string port in ports)
                    {
                        try
                        {
                            m_serialport = new SerialPort(port);

                            m_serialport.BaudRate = 115200;
                            m_serialport.DataBits = 8;
                            m_serialport.Parity = Parity.None;
                            m_serialport.StopBits = StopBits.One;
                            m_serialport.Handshake = Handshake.None;

                            m_serialport.Open();

                            m_serialport.Write("*IDN?\n");
                            Thread.Sleep(200);
                            string recv_str = m_serialport.ReadExisting();
                            if (recv_str.Contains("E-517"))
                            {
                                // Found Device
                                m_serialport.Write("ONL 1 1 2 1\n");
                                Thread.Sleep(100);
                                m_serialport.Write("SVO 1 1 2 1\n");
                                Thread.Sleep(100);
                                m_serialport.Write("MOV 1 0 2 0\n");
                                Thread.Sleep(100);

                                m_distx = 0.0;
                                m_disty = 0.0;

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
            }
        }

        ~smbStage()
        {
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    CFocus.MCL_ReleaseAllHandles();
                    break;
                case StageType.PI_ZSTAGE:
                    m_serialport.Close();
                    break;
                case StageType.PI_PIEZOMIRROR:
                    m_serialport.Close();
                    break;
                case StageType.ASI_MS2000:
                    m_serialport.Close();
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    GCS2.CloseConnection(m_iControllerId);
                    break;
            }
        }

        public void MoveToDist(double dist, uint axis)
        {
            // dist: in um
            // axis: 1 means x, 2 means y, and 3 means z
            //       or 1 means theta, 2 means phi
            switch (m_stage)
            {
                case StageType.MCL_CFOCUS:
                    if (axis != 3) return;
                    CFocus.MCL_SingleWriteN(dist, axis, m_handle);
                    m_distz = dist;
                    break;
                case StageType.PI_ZSTAGE:
                    if (axis != 3) return;
                    m_serialport.Write(String.Format("MOV A{0:0.000}\n", dist));
                    m_distz = dist;
                    break;
                case StageType.PI_XYZNANOSTAGE:
                    string axistring;
                    if (axis == 1) {
                        axistring = "1";
                        m_distx = dist;
                    } else if (axis == 2) {
                        axistring = "2";
                        m_disty = dist;
                    } else {
                        axistring = "3";
                        m_distz = dist;
                    }
                    GCS2.MOV(m_iControllerId, axistring, new double [] { dist });
                    break;
                case StageType.PI_PIEZOMIRROR:
                    if (axis == 1)
                    {
                        m_serialport.Write(String.Format("MOV 1 "));
                        m_distx = dist;
                    }
                    else
                    {
                        m_serialport.Write(String.Format("MOV 2 "));
                        m_disty = dist;
                    }
                    m_serialport.Write(String.Format("{0:0.000}\n", dist));
                    break;
                case StageType.ASI_MS2000:
                    if (axis == 1) {
                        m_distx = dist;
                    } else if (axis == 2) {
                        m_disty = dist;
                    } else {
                        m_distz = dist;
                    }
                    m_serialport.Write(String.Format("M X={0:0} Y={1:0} Z={2:0}\r", m_distx, m_disty, m_distz));
                    break;
            }
        }

    }
}
