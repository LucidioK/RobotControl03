﻿namespace RobotControl.ClassLibrary
{
    public static class ClassFactory
    {
        public static IRobotCommunication CreateRobotCommunication(RobotCommunicationParameters parameters) => new RobotCommunication(parameters);
        public static IImageRecognitionFromCamera CreateImageRecognitionFromCamera(bool useGPU) => new ImageRecognitionFromCamera(useGPU);
    }
}
