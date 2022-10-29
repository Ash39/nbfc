using System.ServiceModel;

namespace StagWare.FanControl.Service
{
    public interface IFanControlService
    {
        void SetTargetFanSpeed(float value, int fanIndex);

        FanControlInfo GetFanControlInfo();

        void Start(bool readOnly);

        void Stop();

        void SetConfig(string uniqueConfigId);

        string[] GetConfigNames();

        string[] GetRecommendedConfigs();
    }
}
