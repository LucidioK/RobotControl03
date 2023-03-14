# RobotControl02
Simple robot control with Arduino and C#.

This is a robot that detects objects via AI, then rotates towards the object and charges towards the object.

Here's a video showing it trying to find a bottle, then charging:
https://1drv.ms/v/s!AvfQ-3ihexuLxtBvqYqCb8fvBgffRg?e=5s4wt4

![Robot02](https://user-images.githubusercontent.com/1053501/167752068-96307dea-36e6-46ed-a36d-6fc3354e860e.jpg) ![Robot03](https://user-images.githubusercontent.com/1053501/167752089-615b9807-a7fa-40a2-a886-9ea1e767858d.jpg)

Here is the physical diagram:
![RobotDiagram](https://user-images.githubusercontent.com/1053501/167751524-b4ce5a1c-0eb4-47c1-a0aa-5f73831ad90d.jpg)

The diagram is in file RobotDiagram.fzz.

Folder SmartRobot02 has the Arduino code.
Folder RobotControl.ClassLibrary has the AI and PC/Robot Communication.
Folder RobotControl.UI has the UI.

## How to build
### For the C# part
For the C# part, that runs on the PC, open RobotControl02.sln on Visual Studio then build.

### For the Arduino part
You should first install the libraries that are used:

1. Open Arduino, then open the Library Manager (Tools/Manage Libraries, or Ctrl+Shift+I)
   1. Search and install these libraries:
      * Adafruit LSM303DLH Mag
      * VL53L0X Pololu
      * L298N Andrea
      * Adafruit LSM303 Accel
1. Open the file SmartRobot02.ino from folder SmartRobot02, compile and upload to your robot.

## To use the CUDA GPU:

1. Follow this: https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/install-gpu-model-builder
1. Download https://developer.nvidia.com/compute/machine-learning/cudnn/secure/8.0.5/10.1_20201106/cudnn-10.1-windows10-x64-v8.0.5.39.zip
1. Extract all files under cuda\bin and copy to %CUDA_PATH%\bin
1. Install Anaconda https://docs.anaconda.com/anaconda/install/index.html
1. Open an Anaconda command prompt, then run:
  * `conda install -c "nvidia/label/cuda-11.8.0" libcublas`
  * `copy %userprofile%\anaconda3\pkgs\libcublas-dev-11.9.2.110-0\bin\*.dll %CUDA_PATH%\bin`
  * `conda install -c "nvidia/label/cuda-11.6.2" cuda-cudart-dev`
  * `copy %userprofile%\anaconda3\pkgs\cuda-cudart-dev-11.6.55-0\bin\*.dlls %CUDA_PATH%\bin`

