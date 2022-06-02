using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RobotControl.ClassLibrary
{
    public class ImageRecognitionFromCameraUtilities
    {
        protected ParameterizedThreadStart cameraWorkerProc;
        protected Thread initializeImageRecognitionFromCameraThread;
        protected bool imageRecognitionFromCameraInitialized;

        public ImageRecognitionFromCameraUtilities()
        {
        }

        public bool ImageRecognitionFromCameraInitialized
        {
            get => imageRecognitionFromCameraInitialized;
            set
            {
                imageRecognitionFromCameraInitialized = value;
            }
        }

        public IImageRecognitionFromCamera ImageRecognitionFromCamera { get; set; }

        protected virtual void InitializeImageRecognitionFromCameraProc(object obj)
        {
            var irfc = ClassFactory.CreateImageRecognitionFromCamera();
            ImageRecognitionFromCamera = irfc;
            ImageRecognitionFromCameraInitialized = true;
        }

        public static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap src)
        {
            var ms = new MemoryStream();
            var im = new BitmapImage();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            im.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            im.StreamSource = ms;
            im.EndInit();
            return im;
        }

        public static void UpdateImage(System.Windows.Controls.Image image, System.Drawing.Bitmap bitmap)
            => image.Dispatcher.Invoke(() => image.Source = BitmapToBitmapImage(bitmap));

    }
}
