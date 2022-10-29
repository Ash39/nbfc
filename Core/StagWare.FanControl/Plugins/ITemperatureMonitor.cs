namespace StagWare.FanControl.Plugins
{
    public interface ITemperatureMonitor : IFanControlPlugin
    {
        string TemperatureSourceDisplayName { get; }
        double Temperature { get; }
        string VendorName { get; }
        void PollTemperature();
    }
}
