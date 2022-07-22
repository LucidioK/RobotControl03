using RobotControl.ClassLibrary;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

namespace RobotControl.UI3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ImageRecognitionFromCameraUtilities utilities;
        protected Thread cameraWorkerThread;
        public MainWindow() : base()
        {
            InitializeComponent();
            utilities = new ImageRecognitionFromCameraUtilities();
            ImageRecognition.Start();
        }

        private static int cameraContinue = 0;
        private string cameraButtonLabel = "";

        private ObservableCollection<string> cameraItemsToRecognizeList = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<string> CameraItemsToRecognizeList
        { 
            get => cameraItemsToRecognizeList;
            set
            {
                cameraItemsToRecognizeList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraItemsToRecognizeList)));
            }
        }

        private void CameraStartRecognition_Click(object sender, RoutedEventArgs e)
        {

            if (CameraStartRecognition.Content.ToString() == "Stop")
            {
                CameraStartRecognition.Content = cameraButtonLabel;
                Interlocked.Exchange(ref cameraContinue, 0);
                SetMotor(this, 0, 0);
            }
            else
            {
                cameraWorkerThread = new Thread(CameraWorkerProc) { Priority = ThreadPriority.AboveNormal };
                cameraWorkerThread.Start(this);

                cameraButtonLabel = CameraStartRecognition?.Content?.ToString() ?? "None";
                if (CameraStartRecognition != null) CameraStartRecognition.Content = "Stop";
                Interlocked.Exchange(ref cameraContinue, 1);
            }
        }

        private static void CameraWorkerProc(object? obj)
        {
            var thisWindow = obj as MainWindow;
            while (thisWindow != null)
            {
                if (CameraShouldContinue())
                {
                    while (CameraShouldContinue())
                    {
                        string[] itemsToRecognize = GetItemsToRecognize(thisWindow);

                        var imageData = thisWindow.ImageRecognition.Get(itemsToRecognize);
                        var objectPosition = imageData.XDeltaProportionFromBitmapCenter * 100;
                        DisplayObjectPosition(thisWindow, objectPosition);
                        MoveRobotAccordingToObjectPosition(thisWindow, imageData, objectPosition);
                    }
                }
                else
                {
                    DisplayWaitingMessage(thisWindow);
                }
            }
        }

        private static string[] GetItemsToRecognize(MainWindow? thisWindow)
        {
            string[] itemsToRecognize = Array.Empty<string>();
            thisWindow?.Dispatcher.Invoke(() =>
            {
                itemsToRecognize = thisWindow.CameraItemsToRecognizeList.ToArray();
                if (itemsToRecognize != null && itemsToRecognize.Length > 0)
                {
                    thisWindow.ImageRecognition.LabelsToDetectText = string.Join(",", itemsToRecognize);
                }
            });

            return itemsToRecognize;
        }

        private static void DisplayWaitingMessage(MainWindow? thisWindow)
        {
            thisWindow?.Dispatcher.Invoke(() =>
            {
                var message = thisWindow.ImageRecognition.ImageRecognitionFromCameraInitialized ?
                    (thisWindow.CameraStartRecognition.IsEnabled ? "Ready to Start Recognition " : "Please select what to find ")
                    : "Waiting to initialize camera ";
                thisWindow.CameraStatusText.Text = $"{message} {DateTime.Now.ToLongTimeString()}";
            });
            Thread.Sleep(500);
        }

        private static void MoveRobotAccordingToObjectPosition(MainWindow thisWindow, ImageRecognitionFromCameraResult imageData, float objectPosition)
        {
            int L=0, R=0, T=0, SL=0, SR=0, ST=0;
            thisWindow.Dispatcher.Invoke(() =>
            {
                L = int.Parse(thisWindow.LurchPower.Text);
                R = int.Parse(thisWindow.LurchPower.Text);
                T = int.Parse(thisWindow.LurchTime.Text);
                SL = int.Parse(thisWindow.ScanLPower.Text);
                SR = int.Parse(thisWindow.ScanRPower.Text);
                ST = int.Parse(thisWindow.ScanTime.Text);
            });

            bool scanning = false;
            if (objectPosition < -5 || !imageData.HasData) // object is to the left, or nothing found
            {
                L = -SL;
                R = SR;
                T = ST;
                scanning = true;
            }
            else if (objectPosition > 5) // object is to the right
            {
                L = SL;
                R = -SR;
                T = ST;
                scanning = true;
            }

            if (scanning && imageData.HasData)
            {
                T /= 2;
            }

            SetMotor(thisWindow, L, R);
            Thread.Sleep(T);
            SetMotor(thisWindow, 0, 0);
            Thread.Sleep(150);
        }

        private static void SetMotor(MainWindow thisWindow, int L, int R) =>
            thisWindow.Dispatcher.Invoke(() => thisWindow.RobotCommunication.Write($"M{ToHex(L)}{ToHex(R)}"));

        private static string ToHex(int n) =>
            $"{(n >= 0 ? "+" : "-")}{Math.Abs(n):X2}";

        private static void DisplayObjectPosition(MainWindow? thisWindow, float objectPosition)
        {
            string positionText = "!!!";
            if (objectPosition < -5) // object is to the left
            {
                positionText = "<-";
            }
            else if (objectPosition > 5) // object is to the right
            {
                positionText = "->";
            }

            thisWindow?.Dispatcher.Invoke(() =>
            {
                thisWindow.CameraStatusText.Text = positionText;
            });
        }

        private static bool CameraShouldContinue() =>
            Interlocked.And(ref cameraContinue, 1) == 1;

        private void CameraItemsToRecognize_RoutedEventHandler(object sender, RoutedEventArgs e)
        {
            var labels = new List<string>();
            foreach (var item in CameraItemsToRecognize.Items)
            {
                var cb = item as CheckBox;
                if (cb != null && cb.IsChecked.GetValueOrDefault())
                {
                    labels.Add(cb.Content.ToString());
                }
            }

            CameraStartRecognition.IsEnabled = labels.Any() && RobotCommunication.IsConnected;
            CameraItemsToRecognizeList = new ObservableCollection<string>(labels);
        }

        private async void CalibrationGetCompassDeclination_Click(object sender, RoutedEventArgs e)
        {
            float lat,lon;
            if (float.TryParse(CalibrationLatitude.Text, out lat) && float.TryParse(CalibrationLongitude.Text, out lon))
            {
                var year     = DateTime.Now.Year;
                var month    = DateTime.Now.Month;
                var day      = DateTime.Now.Day;
                var NS       = lat > 0 ? "N" : "S";
                var EW       = lon > 0 ? "E" : "W";
                lat          = Math.Abs(lat);
                lon          = Math.Abs(lon);

                var url      = $"https://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination?browserRequest=true&magneticComponent=d&key=EAU2y&lat1={lat}&lat1Hemisphere={NS}&lon1={lon}&lon1Hemisphere={EW}&model=WMM&startYear={year}&startMonth={month}&startDay={day}&resultFormat=json";
                var client   = new HttpClient();
                var request  = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await client.SendAsync(request);
                var content  = await response.Content.ReadAsStringAsync();
                var regex    = new Regex(@"""declination"":\s*(\-?[0-9]+\.[0-9]+)");
                var match    = regex.Match(content);
                CalibrationDeclinationAngle.Text = match.Captures[1].Value;
            }
            else
            {
                _ = MessageBox.Show("Invalid Latitude or Longitude value.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CalibrationTurnPowerExperiment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CalibrationMoveAheadPowerExperiment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CalibrationLMultiplierExperiment_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
