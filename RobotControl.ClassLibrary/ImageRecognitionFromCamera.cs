using Microsoft.ML;
using Microsoft.ML.Data;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using RobotControl.ClassLibrary.ONNXImplementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RobotControl.ClassLibrary
{
    internal class ImageRecognitionFromCamera : IImageRecognitionFromCamera
    {
        private TinyYoloModel tinyYoloModel;
        private OnnxModelConfigurator onnxModelConfigurator;
        private OnnxOutputParser onnxOutputParser;
        private PredictionEngine<ImageInputData, TinyYoloPrediction> tinyYoloPredictionEngine;
        private VideoCapture videoCapture;
        private bool flipY;
        private Thread retrieveFramesFromVideoCaptureThread;
        private long latestFramePosition = 0;
        private object retrieveFramesFromVideoCaptureThreadLock = new object();
        private Mat latestFrame = new Mat();
        private readonly Mat[] latestFrames;

        public ImageRecognitionFromCamera(bool useGPU)
        {
            latestFrames             = new Mat[16];
            flipY                    = true;
            var onnxFilePath = Directory.EnumerateFiles(".", "*.onnx").First();
            if (string.IsNullOrEmpty(onnxFilePath))
            {
                throw new FileNotFoundException($"Could not find any onnx file in the current folder {Directory.GetCurrentDirectory()}");
            }
            tinyYoloModel            = new TinyYoloModel(onnxFilePath);
            onnxModelConfigurator    = new OnnxModelConfigurator(tinyYoloModel, useGPU);
            onnxOutputParser         = new OnnxOutputParser(tinyYoloModel);
            tinyYoloPredictionEngine = onnxModelConfigurator.GetMlNetPredictionEngine<TinyYoloPrediction>();
        }

        public bool Open(string cameraId)
        {
            var opened = cameraId.All(c => c >= '0' && c <= '9') 
                ? OpenDeviceCamera(cameraId)
                : OpenIPCamera(cameraId);

            if (!opened)
            {
                throw new ArgumentException($"Could not open camera {cameraId}");
            }

            return opened;
        }

        public ImageRecognitionFromCameraResult Get(string[] labelsOfObjectsToDetect)
        {
            var frame  = new Mat();
            var result = GetEmptyImageRecognitionFromCameraResult();

            lock (retrieveFramesFromVideoCaptureThreadLock)
            {
                frame = latestFrames[latestFramePosition];
            }

            if (frame == null || frame.Empty())
            {
                return result;
            }

            if (flipY) { frame.Flip(FlipMode.Y); }
            result.Bitmap     = BitmapConverter.ToBitmap(frame);
            float[] labels    = Predict(frame);
            var filteredBoxes = onnxOutputParser
                                .FilterBoundingBoxes(onnxOutputParser.ParseOutputs(labels), 5, 0.5f)
                                .Where(b => labelsOfObjectsToDetect.Any(l => l.Equals(b.Label, StringComparison.InvariantCultureIgnoreCase)))
                                .ToList();
            if (filteredBoxes.Count == 0)
            {
                return result;
            }

            var highestConfidence    = filteredBoxes.Select(b => b.Confidence).Max();
            var highestConfidenceBox = filteredBoxes.First(b => b.Confidence == highestConfidence);
            var bbdfb                = BoundingBoxDeltaFromBitmap.FromBitmap(result.Bitmap.Width, result.Bitmap.Height, highestConfidenceBox);
            var dimensions           = HighlightDetectedObject(result.Bitmap, highestConfidenceBox, bbdfb);
            result.HasData           = true;
            result.Label             = dimensions + $", label={highestConfidenceBox.Label}";
            result.XDeltaProportionFromBitmapCenter = bbdfb.XDeltaProportionFromBitmapCenter;

            return result;
        }

        private ImageRecognitionFromCameraResult GetEmptyImageRecognitionFromCameraResult() =>
            new ImageRecognitionFromCameraResult
            {
                HasData = false,
                ImageRecognitionFromCamera = this,
            };

        private void PrintTimeSpans(IList<DateTime> times, IList<string> labels) 
        {
            string s = "";
            for (var i = 1; i < times.Count; i++) 
            {
                var elapsed = times[i] - times[i - 1];
                s += labels[i] + ":" + (int)elapsed.TotalMicroseconds + " ";
            }
            System.Diagnostics.Debug.WriteLine(s);
        }
        public async Task<ImageRecognitionFromCameraResult> GetAsync(string[] labelsOfObjectsToDetect) => 
            await Task.Run(() => Get(labelsOfObjectsToDetect));

        public void Dispose()
        {
            videoCapture?.Dispose();
            tinyYoloPredictionEngine?.Dispose();
        }

        private float[] Predict(Mat frame) =>
            tinyYoloPredictionEngine
                .Predict(new ImageInputData { Image = MLImage.CreateFromStream(frame.ToMemoryStream()) })
                .PredictedLabels;

        private bool OpenIPCamera(string cameraId)
        {
            bool opened;
            videoCapture = new VideoCapture(cameraId, VideoCaptureAPIs.FFMPEG);
            opened       = videoCapture.Open(cameraId, VideoCaptureAPIs.FFMPEG);
            if (opened)
            {
                retrieveFramesFromVideoCaptureThread = new Thread(RetrieveFramesFromVideoCaptureThreadProc);
                retrieveFramesFromVideoCaptureThread.Start();
            }

            return opened;
        }

        private bool OpenDeviceCamera(string cameraId)
        {
            bool opened;
            var intId    = int.Parse(cameraId);
            videoCapture = new VideoCapture(intId);
            opened       = videoCapture.Open(intId);
            return opened;
        }

        private static string HighlightDetectedObject(Bitmap bitmap, BoundingBox box, BoundingBoxDeltaFromBitmap bbdfb)
        {
            var x = box.Dimensions.X * bbdfb.CorrX;
            var y = box.Dimensions.Y * bbdfb.CorrY;
            var w = box.Dimensions.Width * bbdfb.CorrX;
            var h = box.Dimensions.Height * bbdfb.CorrY;
            var midX = x + (w / 2);
            var midY = y + (h / 2);
            if (w != 0)
            {
                using (var gr = Graphics.FromImage(bitmap))
                {
                    gr.DrawRectangle(new Pen(Color.Red, 3), x, y, w, h);
                    gr.DrawLine(new Pen(Color.Green, 2), 0, midY, bitmap.Width - 1, midY);
                    gr.DrawLine(new Pen(Color.Green, 2), midX, 0, midX, bitmap.Height - 1);
                }
            }
            return $"x:{(int)x}, y:{(int)y}, w:{(int)w}, h:{(int)h}";
        }

        private void RetrieveFramesFromVideoCaptureThreadProc(object obj)
        {
            while (true)
            {
                var nextFramePosition           = (Interlocked.Read(ref latestFramePosition) + 1) % latestFrames.LongLength;
                latestFrames[nextFramePosition] = videoCapture.RetrieveMat();
                Interlocked.Exchange(ref latestFramePosition, nextFramePosition);
            }
        }
    }
}
