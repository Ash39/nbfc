using System.Runtime.Serialization;

namespace StagWare.FanControl.Service
{
    public class FanControlInfo
    {
        public bool Enabled { get; set; }

        public bool ReadOnly { get; set; }

        public FanStatus[] FanStatus { get; set; }
        
        public string SelectedConfig { get; set; }
    }
}
