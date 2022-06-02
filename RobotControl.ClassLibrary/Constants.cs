using System;
using System.Collections.Generic;
using System.Text;

namespace RobotControl.ClassLibrary
{
    public static class Constants
    {
        public static string[] YOLORecognitionLabels =
        {
                "aeroplane"  , "bicycle", "bird" , "boat"     , "bottle"   ,
                "bus"        , "car"    , "cat"  , "chair"    , "cow"      ,
                "diningtable", "dog"    , "horse", "motorbike", "person"   ,
                "pottedplant", "sheep"  , "sofa" , "train"    , "tvmonitor"
        };

        public static string OnnxFilePath = "TinyYolo2_model.onnx";
    }
}
