using Microsoft.ML.Data;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal class TinyYoloPrediction : IOnnxObjectPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels { get; set; }
    }
}
