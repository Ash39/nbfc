using System.ComponentModel.Composition;
using System.Diagnostics;
using StagWare.FanControl.Plugins;

namespace StagWare.Plugins.Generic;

[Export(typeof(ITemperatureMonitor))]
[FanControlPluginMetadata(
    "Ash39.Plugins.NvidiaTemperatureLinux",
    SupportedPlatforms.Unix,
    SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
public class NvidiaTemperatureLinux : ITemperatureMonitor
{

    private Process nvidiasmi;
    private int gpuCount;
    
    public bool IsInitialized { get; private set;  }
    public void Initialize()
    {
        if (!this.IsInitialized)
        {
            try
            {
                nvidiasmi = new Process();
                nvidiasmi.StartInfo.FileName = "nvidia-smi";
                nvidiasmi.StartInfo.Arguments = "--query-gpu=count --format=csv,noheader";
                nvidiasmi.StartInfo.RedirectStandardOutput = true;
                nvidiasmi.Start();

                gpuCount = int.Parse(nvidiasmi.StandardOutput.ReadLine());
                
                nvidiasmi.WaitForExit();

                IsInitialized = nvidiasmi.ExitCode == 0;
            }
            catch
            {
            }
        }
    }

    public string TemperatureSourceDisplayName => GetGpuInfo().name;
    public double Temperature { get; private set; }
    public string VendorName => "Nvidia Device";
    public void PollTemperature()
    {
        Temperature = GetGpuInfo().temperature;
    }

    private (int index, string name, double temperature) GetGpuInfo()
    {

        try
        {
            string[] values = null;
            
            nvidiasmi.StartInfo.Arguments = "--query-gpu=index,gpu_name,temperature.gpu,fan.speed --format=csv,noheader";
            nvidiasmi.Start();
            
            for (int i = 0; i < gpuCount; i++)
            {
                values = nvidiasmi.StandardOutput.ReadLine().Split(',');

                if (!int.TryParse(values[3], out _))
                {
                    break;
                }
            }
        
            nvidiasmi.WaitForExit();

            return (int.Parse(values[0]), values[1], double.Parse(values[2]));
        }
        catch
        {
            return GetGpuInfo();
        }
    }

    public void Dispose()
    {
        nvidiasmi.Dispose();
    }

}