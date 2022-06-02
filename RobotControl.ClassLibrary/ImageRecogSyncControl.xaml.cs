using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace RobotControl.ClassLibrary
{
    /// <summary>
    /// Interaction logic for ImageRecogSyncControl.xaml
    /// </summary>
    public partial class ImageRecogSyncControl : UserControl, INotifyPropertyChanged
    {

        protected Thread cameraWorkerThread;
        protected Thread initializeImageRecognitionFromCameraThread;
        private bool imageRecognitionFromCameraInitialized;
        private string labelsToDetectText;
        private object startLock = new object();
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ImageRecognitionFromCameraInitialized
        {
            get => imageRecognitionFromCameraInitialized;
            set
            {
                imageRecognitionFromCameraInitialized = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageRecognitionFromCameraInitialized)));
            }
        }

        public static readonly DependencyProperty LabelsToDetectTextProperty = DependencyProperty.Register(
                                                         name: "LabelsToDetectText",
                                                         propertyType: typeof(string),
                                                         ownerType: typeof(ImageRecogSyncControl),
                                                         typeMetadata: new PropertyMetadata(
                                                         new PropertyChangedCallback(LabelsToDetectTextPropertyChanged)));

        private static void LabelsToDetectTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((ImageRecogSyncControl)sender).LabelsToDetectText = (string)e.NewValue;

        public string LabelsToDetectText
        {
            get => labelsToDetectText;
            set
            {
                labelsToDetectText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LabelsToDetectText)));
            }
        }

        public IImageRecognitionFromCamera ImageRecognitionFromCamera { get; protected set; }

        public ImageRecogSyncControl()
        {
            InitializeComponent();
        }

        public void Start()
        {
            lock (startLock)
            {
                if (initializeImageRecognitionFromCameraThread == null)
                {
                    initializeImageRecognitionFromCameraThread = new Thread(InitializeImageRecognitionFromCameraProc);
                    initializeImageRecognitionFromCameraThread.Start();
                }
            }
        }

        protected virtual void InitializeImageRecognitionFromCameraProc(object obj)
        {
            Dispatcher.Invoke(() => Status.Text = "Initializing Image Recognition.");
            var irfc = ClassFactory.CreateImageRecognitionFromCamera();
            Dispatcher.Invoke(() =>
            {
                ImageRecognitionFromCamera = irfc;
                ImageRecognitionFromCameraInitialized = true;
            });
        }

        public ImageRecognitionFromCameraResult Get(string[] labels)
        {
            WaitUntilImageRegognitionFromCameraIsInitialized();
            ImageRecognitionFromCameraResult imageData = new ImageRecognitionFromCameraResult();

            var start = DateTime.Now;
            Dispatcher.Invoke(() =>
            {
                imageData = ImageRecognitionFromCamera.Get(labels);
                using (var gr = Graphics.FromImage(imageData.Bitmap))
                {
                    gr.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Yellow, 10), 5, 5, imageData.Bitmap.Width - 5, imageData.Bitmap.Height - 5);
                }
                Image.Source = ImageRecognitionFromCameraUtilities.BitmapToBitmapImage(imageData.Bitmap);
                Status.Text = $"{(DateTime.Now - start).TotalMilliseconds}ms";
            });

            return imageData;
        }

        private void WaitUntilImageRegognitionFromCameraIsInitialized()
        {
            var start = DateTime.Now;

            for (bool imageRecognitionFromCameraInitialized = false; !imageRecognitionFromCameraInitialized;)
            {
                Dispatcher.Invoke(() => imageRecognitionFromCameraInitialized = ImageRecognitionFromCameraInitialized);
                if (!imageRecognitionFromCameraInitialized)
                {
                    Dispatcher.Invoke(() => Status.Text = $"Initializing Image Recognition for {(DateTime.Now - start).TotalSeconds} seconds.");
                    Thread.Sleep(500);
                }
            }
        }
    }
}
