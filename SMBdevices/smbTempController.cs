using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SMBdevices
{
    public class smbTempController
    {
        public string m_port;
        private SerialPort m_serialport;



        public enum TempConType
        {
            LCI_CU109
        }

        public static string GetTempConName(TempConType tempcon)
        {
            switch (tempcon)
            {
                case TempConType.LCI_CU109:
                    return "LCI CU-109";
            }
            return "Unknown Temp Controller";
        }

        public smbTempController(TempConType tempcon, string port)
        {
            m_port = port;
            m_serialport = new SerialPort(port);
            
        }
    }
}
