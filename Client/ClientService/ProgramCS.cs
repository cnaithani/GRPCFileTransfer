using ClientService.Modules.Classes;
using ClientService.Modules.Interfaces;
using ClientService.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

string AppName = "GRPC - Client Service";

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
Log.Logger = CreateSerilogLogger(config, AppName);

Log.Information("Starting web host ({ApplicationContext})...", AppName);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<IClientJobs, ClientJobs>();

var app = builder.Build();

 static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", appName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

// Configure the HTTP request pipeline.
app.MapGrpcService<JobService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
