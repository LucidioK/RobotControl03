using System;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    public interface IImageRecognitionFromCamera : IDisposable
    {
        bool Open(string cameraId);
        Task<ImageRecognitionFromCameraResult> GetAsync(string[] labelsOfObjectsToDetect);
        ImageRecognitionFromCameraResult Get(string[] labelsOfObjectsToDetect);
    }
}