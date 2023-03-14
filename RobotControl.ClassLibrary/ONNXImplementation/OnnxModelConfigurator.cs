using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RobotControl.ClassLibrary.ONNXImplementation
{


    internal class OnnxModelConfigurator
    {
        private readonly MLContext mlContext = new MLContext();
        private readonly ITransformer mlModel;
        private readonly IDataView dataView;

        public OnnxModelConfigurator(IOnnxModel onnxModel, bool useGPU)
        {
            dataView = mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());
            mlModel  = SetupMlNetModel(onnxModel, useGPU);
        }

        private ITransformer SetupMlNetModel(IOnnxModel onnxModel, bool useGPU) =>
            mlContext
                .Transforms
                    .ResizeImages(
                        resizing: ImageResizingEstimator.ResizingKind.Fill,
                        outputColumnName: onnxModel.ModelInput,
                        imageWidth: ImageSettings.imageWidth,
                        imageHeight: ImageSettings.imageHeight,
                        inputColumnName: nameof(ImageInputData.Image))

                    .Append(mlContext.Transforms.ExtractPixels(
                        outputColumnName: onnxModel.ModelInput))

                    .Append(mlContext.Transforms.ApplyOnnxModel(
                        modelFile: onnxModel.ModelPath,
                        outputColumnName: onnxModel.ModelOutput,
                        inputColumnName: onnxModel.ModelInput,
                            gpuDeviceId: 0,
                            fallbackToCpu: !useGPU))
                    .Fit(dataView);

        public PredictionEngine<ImageInputData, T> GetMlNetPredictionEngine<T>()
            where T : class, IOnnxObjectPrediction, new() => mlContext.Model.CreatePredictionEngine<ImageInputData, T>(mlModel);

        public void SaveMLNetModel(string mlnetModelFilePath) => mlContext.Model.Save(mlModel, null, mlnetModelFilePath);
    }
}
