using System.Drawing;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    public class BoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; }

        public string Label                     { get; set; }

        public float Confidence                 { get; set; }

        public Color BoxColor                   { get; set; }

        public RectangleF Rect    => new(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height);

        public string Description => $"{Label} ({Confidence * 100:0}%)";
    }
}
