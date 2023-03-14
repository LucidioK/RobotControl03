using System.IO;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal class TinyYoloModel : IOnnxModel
    {
        public string ModelPath         { get; private set; }
        public string ModelInput        { get; } = "image";
        public string ModelOutput       { get; } = "grid";
        public (float, float)[] Anchors { get; } = { (1.08f, 1.19f), (3.42f, 4.41f), (6.63f, 11.38f), (9.42f, 5.11f), (16.62f, 10.52f) };
        public string[] Labels => Constants.YOLORecognitionLabels;

        public TinyYoloModel(string modelPath)
        {
            if (File.Exists(modelPath))
            {
                ModelPath = Path.GetFullPath(modelPath);
            }
            else
            {
                throw new FileNotFoundException($"Could not find {modelPath} on {Path.GetFullPath(".")}");
            }
        }
    }
}
