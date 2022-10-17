using System.Net.Http.Headers;
using System.Net.Http.Json;
using StagWare.FanControl;
using StagWare.FanControl.Service;

namespace NbfcCli;

public class FanControlServiceClient : IDisposable
{
    private HttpClient client;
    
    public FanControlServiceClient()
    {
        client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:34324/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }


    public void Dispose()
    {
        client.Dispose();
    }

    public void Start(bool readOnly)
    {
        client.PostAsJsonAsync("FanControlService/Start",readOnly).Wait();
    }
    
    public void Stop()
    {
        client.PostAsync("/FanControlService/Stop", null).Wait();
    }

    public string[]? GetRecommendedConfigs()
    {
        return client.GetFromJsonAsync<string[]>("/FanControlService/GetRecommendedConfigs").Result;
    }

    public string[]? GetConfigNames()
    {
        return client.GetFromJsonAsync<string[]>("/FanControlService/GetConfigNames").Result;
    }

    public FanControlInfo GetFanControlInfo()
    {
        return client.GetFromJsonAsync<FanControlInfo>("/FanControlService/GetFanControlInfo").Result;
    }

    public void SetConfig(string configName)
    {
        client.PostAsJsonAsync("FanControlService/SetConfig",configName).Wait();
    }

    public void SetTargetFanSpeed(float speed, int idx)
    {
        TargetFanSpeed targetFanSpeed = new TargetFanSpeed()
        {
            speed = speed,
            idx = idx
        };
        client.PostAsJsonAsync("FanControlService/SetTargetFanSpeed",targetFanSpeed).Wait();
    }
    
}