using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace RobotControl.ClassLibrary
{
    public static class ImageRecognitionFromCameraUtilities
    {
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
    }
}
