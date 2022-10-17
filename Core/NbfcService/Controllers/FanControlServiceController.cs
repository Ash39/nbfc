using Microsoft.AspNetCore.Mvc;
using StagWare.FanControl;
using StagWare.FanControl.Service;

namespace NbfcService.Controllers;

[Route("[controller]")]
[ApiController]
public class FanControlServiceController : Controller
{
    private readonly IServiceProvider _serviceProdiver;
    
    
    public FanControlServiceController(IServiceProvider serviceProdiver)
    {
        this._serviceProdiver = serviceProdiver;
    }

    [HttpPost("SetTargetFanSpeed")]
    public void SetTargetFanSpeed(TargetFanSpeed targetFanSpeed) {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            fanControlService.SetTargetFanSpeed(targetFanSpeed.speed, targetFanSpeed.idx);
        }
    }
    
    [HttpGet("GetFanControlInfo")]
    public FanControlInfo GetFanControlInfo() {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            return fanControlService.GetFanControlInfo();
        }
        
    }
    
    [HttpPost("Start")]
    public void Start(bool readOnly) {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            fanControlService.Start(readOnly);
        }
        
    }
    
    [HttpPost("Stop")]
    public void Stop() {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            fanControlService.Stop();
        }
        
    }
        
    [HttpPost("SetConfig")]
    public void SetConfig(string uniqueConfigId) {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            fanControlService.SetConfig(uniqueConfigId);
        }
        
    }
    
    [HttpGet("GetConfigNames")]
    public string[] GetConfigNames() {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            return fanControlService.GetConfigNames();
        }
        
    }
    
    [HttpGet("GetRecommendedConfigs")]
    public string[] GetRecommendedConfigs() {
        using (FanControlService fanControlService = _serviceProdiver.GetService<FanControlService>())
        {
            return fanControlService.GetRecommendedConfigs();
        }
    }
}