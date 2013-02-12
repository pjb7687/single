using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace SMBdevices
{
    public class smbPump
    {
        public pumpType m_pump;
        public string m_port;
        private SerialPort m_serialport;

        public double m_minrate;
        public double m_maxrate;
        public double m_minvolume;
        public double m_maxvolume;

        private double m_diameter;
        private double m_rate;
        private double m_volume;
        
        public enum pumpType
        {
            CHEMYX_FUSION,
            HARVARD_2000
        }

        public enum unitType
        {
            ML_PER_MIN = 0,
            ML_PER_HR = 1,
            UL_PER_MIN = 2,
            UL_PER_HR = 3
        }

        public enum runMode
        {
            INFUSION,
            REFILL
        }

        public static string GetPumpName(pumpType pump)
        {
            switch (pump)
            {
                case pumpType.CHEMYX_FUSION:
                    return "Chemyx Fusion Series";
                case pumpType.HARVARD_2000:
                    return "Harvard 2000";
            }
            return "Unknown Pump";
        }

        private void RetrieveParameters()
        {
            string [] p;
            switch (m_pump)
            {
                case pumpType.CHEMYX_FUSION:
                    m_serialport.ReadExisting();
                    m_serialport.Write("view parameter\r\n");
                    Thread.Sleep(200);
                    p = m_serialport.ReadLine().Split(' '); // command

                    p = m_serialport.ReadLine().Trim().Split(' '); // unit
                    if (Convert.ToInt32(p[p.Length - 1]) != 2)
                    {
                        // unit is not ul/mn
                        m_serialport.Write("set units 2\r\n");
                        Thread.Sleep(200);
                        RetrieveParameters();
                        return;
                    }

                    p = m_serialport.ReadLine().Trim().Split(' '); // diameter
                    m_diameter = Convert.ToDouble(p[p.Length - 1]);
                    p = m_serialport.ReadLine().Trim().Split(' '); // rate
                    m_rate = Convert.ToDouble(p[p.Length - 1]);
                    m_serialport.ReadLine(); // primerate ?
                    m_serialport.ReadLine(); // time ?
                    p = m_serialport.ReadLine().Trim().Split(' '); // volume
                    m_volume = Convert.ToDouble(p[p.Length - 1]);
                    m_serialport.ReadExisting();
                    break;
                case pumpType.HARVARD_2000:
                    break;
            }
        }

        public void RunWithSettings(double diameter, unitType units, runMode mode, double volume, double rate) {
            switch (m_pump)
            {
                case pumpType.CHEMYX_FUSION:
                    string cmd = "hexw2 " + ((int)units).ToString() + " " +
                                        ((int)mode).ToString() + " " +
                                        diameter.ToString("0.00000") + " " +
                                        volume.ToString("0.00000") + " " +
                                        rate.ToString("0.00000") + " 0 start\r\n";
                    m_serialport.Write(cmd);
                    Thread.Sleep(200);
                    break;
                case pumpType.HARVARD_2000:
                    break;
            }
        }

        public smbPump(pumpType pump, string port)
        {
            m_pump = pump;
            m_port = port;
            string[] limitvals;
            string test;
            switch (m_pump)
            {
                case pumpType.CHEMYX_FUSION:
                    try {
                        m_serialport = new SerialPort(port);

                        m_serialport.BaudRate = 9600;
                        m_serialport.DataBits = 8;
                        m_serialport.Parity = Parity.None;
                        m_serialport.StopBits = StopBits.One;
                        m_serialport.Handshake = Handshake.None;
                        m_serialport.ReadTimeout = 500;

                        m_serialport.Open();

                        m_serialport.Write("read limit parameter\r\n");
                        Thread.Sleep(200);
                        m_serialport.ReadLine(); // remove commands
                        limitvals = m_serialport.ReadLine().Split(' ');
                        m_maxrate = Convert.ToDouble(limitvals[0].Trim());
                        m_minrate = Convert.ToDouble(limitvals[1].Trim());
                        m_maxvolume = Convert.ToDouble(limitvals[2].Trim());
                        m_minvolume = Convert.ToDouble(limitvals[3].Trim());
                        m_serialport.ReadExisting();
                    } catch {
                        m_serialport.Close();
                        throw new Exception();
                    }
                    break;
                case pumpType.HARVARD_2000:
                    try
                    {
                        m_serialport = new SerialPort(port);

                        m_serialport.BaudRate = 9600;
                        m_serialport.DataBits = 8;
                        m_serialport.Parity = Parity.None;
                        m_serialport.StopBits = StopBits.Two;
                        m_serialport.Handshake = Handshake.None;

                        m_serialport.Open();

                        m_serialport.Write("\rVER\r");
                        Thread.Sleep(200);
                        test = m_serialport.ReadExisting();
                        if (test == "") throw new Exception();
                    }
                    catch
                    {
                        m_serialport.Close();
                        throw new Exception();
                    }
                    break;
            }
        }

        public double GetDiameter()
        {
            RetrieveParameters();
            return m_diameter;
        }

        public double GetVolume()
        {
            RetrieveParameters();
            return m_volume;
        }

        public double GetRate()
        {
            RetrieveParameters();
            return m_rate;
        }

        public runMode GetDirection()
        {
            return runMode.REFILL;
        }

        ~smbPump()
        {
            m_serialport.Close();
        }
    }
}
