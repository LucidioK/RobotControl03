namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal interface IOnnxObjectPrediction
    {
        float[] PredictedLabels { get; set; }
    }
}
