using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.UI3
{
    internal class RobotControlData : INotifyPropertyChanged
    {
        private int lurchTimeValue  = 1000;
        private int scanRPowerValue = 220;
        private int scanLPowerValue = 220;
        private int scanTimeValue   = 100;
        private int lurchPowerValue = 250;
        private string jsonFilePath;
        private string cameraStatus;

        public RobotControlData()
        {
            jsonFilePath = Path.Join(Path.GetTempPath(), nameof(RobotControlData) + ".json"); ;
        }

        public void InitializeWithJSONIfExists()
        {
            if (File.Exists(jsonFilePath))
            {
                var robotControlData = JsonConvert.DeserializeObject<RobotControlData>(File.ReadAllText(jsonFilePath));
                foreach (var property in this.GetType().GetProperties())
                {
                    try
                    {
                        var value = property.GetValue(robotControlData);
                        if (value != null)
                        {
                            property.SetValue(this, value);
                        }
                    }
                    catch (TargetException) { }
                    catch (TargetInvocationException) { }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public int LurchPowerValue { get => lurchPowerValue; set { lurchPowerValue = value; PropChgd(nameof(lurchPowerValue)); } }
        public int ScanRPowerValue { get => scanRPowerValue; set { scanRPowerValue = value; PropChgd(nameof(scanRPowerValue)); } }
        public int ScanLPowerValue { get => scanLPowerValue; set { scanLPowerValue = value; PropChgd(nameof(scanLPowerValue)); } }
        public int ScanTimeValue   { get => scanTimeValue;   set { scanTimeValue   = value; PropChgd(nameof(scanTimeValue));   } }
        public int LurchTimeValue  { get => lurchTimeValue;  set { lurchTimeValue  = value; PropChgd(nameof(lurchTimeValue));  } }
        [JsonIgnore]
        public string CameraStatus { get => cameraStatus;    set { cameraStatus    = value; PropChgd(nameof(cameraStatus));    } }
        private ObservableCollection<string> cameraItemsToRecognizeList = new();

        public ObservableCollection<string> CameraItemsToRecognizeList
        {
            get => cameraItemsToRecognizeList;
            set
            {
                cameraItemsToRecognizeList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraItemsToRecognizeList)));
            }
        }

        private void PropChgd(string propertyName)
        {
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(this));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
