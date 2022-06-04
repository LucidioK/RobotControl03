using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RobotControl.ClassLibrary
{
    /// <summary>
    /// Interaction logic for RobotCommunicationControl.xaml
    /// </summary>
    public partial class RobotCommunicationControl : UserControl, INotifyPropertyChanged
    {

        private string resultText;
        private string commandText;
        private string arduinoCOMPort;
        private IRobotCommunication robotCommunication;
        public RobotCommunicationControl()
        {
            InitializeComponent();
            COMPort.Items.Clear();
            SerialPort.GetPortNames().ToList().ForEach(p => COMPort.Items.Add(p));
//            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

//            foreach (ManagementObject queryObj in searcher.Get())
//            {
//                string s = queryObj["Name"] as string;
//                if (s != null && s.Contains("(COM"))
//                {
//#if DEBUG
//                    foreach (var prop in queryObj.Properties)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"-->RobotCommunicationControl {prop.Name}={prop.Value}");
//                    }
//#endif
//                    string d = queryObj["Description"] as string;
//                    if (d != null && d.Contains("Arduino"))
//                    {
//                        arduinoCOMPort = s;
//                    }
//                }
//            }
        }

        public bool IsConnected => Send.IsEnabled;
        public string CommandText 
        { 
            get => commandText;
            set
            {
                commandText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandText)));
                Send.IsEnabled = value.Any();
            }
        }
        public string ResultText 
        { 
            get => resultText;
            set
            { 
                resultText = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultText)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Write(string s)
        {
            robotCommunication.Write(s);
            var result = robotCommunication.Read();
            Result.Text = result.ToString();
            return Result.Text;
        }

        private async void Connect_ClickAsync(object sender, RoutedEventArgs e)
        {
            var previousCursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            var rc = new RobotCommunicationParameters
            {
                BaudRate = int.Parse(BaudRateComboBox.Text),
                COMPort = COMPort.Text
            };

            robotCommunication = ClassFactory.CreateRobotCommunication(rc);
            await robotCommunication.StartAsync();
            this.Cursor = previousCursor;
            Send.IsEnabled = true;
        }

        private void Send_Click(object sender, RoutedEventArgs e) => Write(Command.Text);

        private void BaudRateComboBoxChanged(object sender, SelectionChangedEventArgs e) { }

        private void COMPortChanged(object sender, SelectionChangedEventArgs e) => Connect.IsEnabled = true;
        private void EnableConnectButtonIfNeeded()
        {
            if (Connect != null)
            {
                Connect.IsEnabled = !string.IsNullOrEmpty(BaudRateComboBox.Text) && !string.IsNullOrEmpty(COMPort.Text);
                System.Diagnostics.Debug.WriteLine($"-->EnableConnectButtonIfNeeded [{BaudRateComboBox.Text}] [{COMPort.Text}][{Connect.IsEnabled}]");
            }
        }
    }
}
