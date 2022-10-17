global using Microsoft.Extensions.Hosting.Systemd;
using StagWare.FanControl.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSystemd();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

FanControlService service = new FanControlService(LoggerFactory.Create((loggingBuilder) =>
{
    loggingBuilder
        .SetMinimumLevel(LogLevel.Trace)
        .AddConsole();
}).CreateLogger<FanControlService>());

builder.Services.AddSingleton<FanControlService>(service);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseAuthorization();

app.MapControllers();

app.Run();
