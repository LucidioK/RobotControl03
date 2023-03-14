namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal interface IOnnxModel
    {
        string           ModelPath   { get; }
        string           ModelInput  { get; }
        string           ModelOutput { get; }

        string[]         Labels      { get; }
        (float, float)[] Anchors     { get; }
    }
}
