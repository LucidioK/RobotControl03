using RobotControl.ClassLibrary;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
            string[] itemsToRecognize = Array.Empty<string>();
            while (thisWindow != null)
            {
                if (CameraShouldContinue())
                {
                    while (CameraShouldContinue())
                    {
                        thisWindow.Dispatcher.Invoke(() =>
                        {
                            itemsToRecognize = thisWindow.CameraItemsToRecognizeList.ToArray();
                            if (itemsToRecognize != null && itemsToRecognize.Length > 0)
                            {
                                thisWindow.ImageRecognition.LabelsToDetectText = string.Join(",", itemsToRecognize);
                            }
                        });

                        var imageData = thisWindow.ImageRecognition.Get(itemsToRecognize);
                            var objectPosition = imageData.XDeltaProportionFromBitmapCenter * 100;
                            string positionText = "!!!";
                            if (objectPosition < -5) // object is to the left
                            {
                                positionText = "<-";
                            }
                            else if (objectPosition > 5) // object is to the right
                            {
                                positionText = "->";
                            }

                        thisWindow.Dispatcher.Invoke(() =>
                        {
                            thisWindow.CameraStatusText.Text = positionText;
                        });
                    }
                }
                else
                {
                    thisWindow.Dispatcher.Invoke(() =>
                    {
                        var message = thisWindow.ImageRecognition.ImageRecognitionFromCameraInitialized ?
                            (thisWindow.CameraStartRecognition.IsEnabled ? "Ready to Start Recognition " : "Please select what to find ")
                            : "Waiting to initialize camera ";
                        thisWindow.CameraStatusText.Text = $"{message} {DateTime.Now.ToLongTimeString()}";
                    });
                    Thread.Sleep(500);
                }
            }
        }

        private static bool CameraShouldContinue()
        {
            var shouldContinue = Interlocked.And(ref cameraContinue, 1);
            return shouldContinue == 1;
        }

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

            CameraStartRecognition.IsEnabled = labels.Any();
            CameraItemsToRecognizeList = new ObservableCollection<string>(labels);
        }
    }
}
