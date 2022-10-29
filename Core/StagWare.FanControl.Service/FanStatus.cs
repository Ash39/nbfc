using System.Runtime.Serialization;

namespace StagWare.FanControl.Service
{
    public class FanStatus
    {
        public string FanDisplayName { get; set; }
        
        public float Temperature { get; set; }

        public string TemperatureSourceDisplayName { get; set; }

        public bool AutoControlEnabled { get; set; }

        public bool CriticalModeEnabled { get; set; }

        public float CurrentFanSpeed { get; set; }

        public float TargetFanSpeed { get; set; }

        public int FanSpeedSteps { get; set; }
    }
}
