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
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class ImageRecogControl : UserControl, INotifyPropertyChanged
    {
        protected Thread cameraWorkerThread;
        protected Thread initializeImageRecognitionFromCameraThread;
        private bool imageRecognitionFromCameraInitialized;
        private string labelsToDetectText;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ImageRecognitionFromCameraInitialized
        {
            get => imageRecognitionFromCameraInitialized;
            set => imageRecognitionFromCameraInitialized = value;
        }

        public static readonly DependencyProperty LabelsToDetectTextProperty = DependencyProperty.Register(
                                                         name: "LabelsToDetectText",
                                                         propertyType: typeof(string),
                                                         ownerType: typeof(ImageRecogControl),
                                                         typeMetadata: new PropertyMetadata(
                                                         new PropertyChangedCallback(LabelsToDetectTextPropertyChanged)));

        private static void LabelsToDetectTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
            ((ImageRecogControl)sender).LabelsToDetectText = (string)e.NewValue;

        public string LabelsToDetectText
        {
            get => labelsToDetectText;
            protected set
            {
                labelsToDetectText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(labelsToDetectText)));
            }
        }

        public IImageRecognitionFromCamera ImageRecognitionFromCamera { get; protected set; }

        public ImageRecogControl()
        {
            InitializeComponent();
        }

        public void Start(string[] labels)
        {
            LabelsToDetectText = string.Join(",", labels);
            initializeImageRecognitionFromCameraThread = new Thread(InitializeImageRecognitionFromCameraProc);
            initializeImageRecognitionFromCameraThread.Start();
            cameraWorkerThread = new Thread(CameraWorkerProc) { Priority = ThreadPriority.AboveNormal };
            cameraWorkerThread.Start();
        }

        public override void BeginInit()
        {
            base.BeginInit();
        }

        protected virtual void InitializeImageRecognitionFromCameraProc(object obj)
        {
            Dispatcher.Invoke(() => Status.Text = "Initializing Image Recognition.");
            var irfc = ClassFactory.CreateImageRecognitionFromCamera();
            Dispatcher.Invoke(() =>
            {
                ImageRecognitionFromCamera = irfc;
                ImageRecognitionFromCameraInitialized = true;
                Status.Text = "";
            });
        }

        private void CameraWorkerProc(object obj)
        {
            WaitUntilImageRegognitionFromCameraIsInitialized();

            string[] labels = null;
            ImageRecognitionFromCameraResult imageData;
            while (true)
            {
                Dispatcher.Invoke(() => labels = LabelsToDetectText.Split(",")); 
                var start = DateTime.Now;
                Dispatcher.Invoke(() =>
                {
                    imageData = ImageRecognitionFromCamera.Get(labels);
                    
                    using (var gr = Graphics.FromImage(imageData.Bitmap))
                    {
                        gr.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Yellow, 10), 5, 5, imageData.Bitmap.Width - 5, imageData.Bitmap.Height - 5);
                    }
                    Status.Text = $"{(DateTime.Now - start).TotalMilliseconds}ms";
                    Image.Source = ImageRecognitionFromCameraUtilities.BitmapToBitmapImage(imageData.Bitmap);
                });
            }
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

            Dispatcher.Invoke(() => Status.Text = "");
        }
    }
}
