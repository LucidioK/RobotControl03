using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal class ImageInputData
    {
        [ImageType(ImageSettings.imageHeight, ImageSettings.imageWidth)]
        public MLImage Image { get; set; }
    }
}
