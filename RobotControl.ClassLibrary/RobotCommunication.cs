
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    internal class RobotCommunication : IRobotCommunication
    {
#if DEBUG
        private const int ReadTimeOut = 1000;
#else
        private const int ReadTimeOut = 200;
#endif
        private const int OpenTentatives = 4;

        private const string SmartRobotId = "SmartRobot03";
        private readonly RobotCommunicationParameters parameters;
        private SerialPort serialPort = null;
        
        private int LFromRobot = -1;
        private int RFromRobot = -1;

        public RobotCommunication(RobotCommunicationParameters parameters) => this.parameters = parameters;

        public string[] PortNames => SerialPort.GetPortNames();

        public void Start()
        {
            bool foundPort = false;
            var ports = PortNames;
            for (int i = 0; i < OpenTentatives && !foundPort; i++)
            {
                if (OpenPort(parameters.COMPort))
                {
                    System.Diagnostics.Debug.WriteLine($"-->RobotCommunication.StartAsync Opened port {parameters.COMPort} <--");
                    return;
                }
                Thread.Sleep(100);
            }

            throw new Exception($"Cannot find SmartRobot04 COM port. Check if the robot is connected to a USB port. Check if Arduino IDE or other app is using the port. Aborting.");
        }

        public string Read()
        {
            for (int i = 0; i < OpenTentatives && serialPort.BytesToRead == 0; i++)
            {
                Thread.Sleep(50);
            }

            if (serialPort.BytesToRead > 0)
            {
                try
                {
                    var result = serialPort.ReadLine();
                    return result;
                }
                catch (TimeoutException)
                { }
                catch (IOException)
                { }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("-->Read nothing to read...");
            }
            return string.Empty;
        }

        public void SetMotors(int l, int r)
        {
            if (LFromRobot != l || RFromRobot != r)
            {
                Write($"M{toHex(l)}{toHex(r)}");
            }
        }


        public void Write(string s) => serialPort.WriteLine(s);

        public void StopMotors() => Write($"M0000");

        public async Task<string> ReadAsync() => await Task.Run(() => Read());
        public async Task SetMotorsAsync(int l, int r) => await Task.Run(() => SetMotors(l, r));
        public async Task StopMotorsAsync() => await Task.Run(() => StopMotors());
        public async Task StartAsync() => await Task.Run(() => Start());
        public async Task WriteAsync(string s) => await Task.Run(() => Write(s));


        public void Dispose()
        {
            serialPort?.Close();
        }

        private string toHex(int n) => $"{((n<0)?"-":"+")}{Math.Abs(n).ToString("0:X2")}";

        private bool OpenPort(string portName)
        {
            ClosePortIfNeeded();
            
            serialPort = new SerialPort(portName, parameters.BaudRate);
            
            // this seems to be important for Arduino:
            serialPort.RtsEnable = true;
            serialPort.ReadTimeout = ReadTimeOut;
            serialPort.Open();
            return serialPort.IsOpen;
        }

        private void ClosePortIfNeeded()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}
