using ClientService.Modules.Classes;
using ClientService.Modules.Interfaces;
using ClientService.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using AgentService.Protos;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using ClientService.Protos;

namespace ClientApp
{
    public class Program
    {
        private static string AppName = "GRPC - Client Service";
        private static JobC.JobCClient clientJob;
        private static JobA.JobAClient agentJob;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables().Build();
            Log.Logger = CreateSerilogLogger(config, AppName);

            Log.Information("Starting web host ({ApplicationContext})...", AppName);

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddSingleton(Log.Logger);
            builder.Services.AddSingleton<IClientJobs, ClientJobs>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<JobService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
            "To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            Task.Run(() => { app.Run();});
            Thread.Sleep(1000);


            /*-----------------*/


            Console.WriteLine(string.Concat(AppName, " Started. Please read instructions and execue commands - ", Environment.NewLine));

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var clientAddress = config["Services:Client"];
            var clientChannelJob = GrpcChannel.ForAddress(clientAddress, new GrpcChannelOptions { HttpHandler = handler });
            clientJob = new JobC.JobCClient(clientChannelJob);

            var agentAddress = config["Services:Agent"];
            var agentChannelJob = GrpcChannel.ForAddress(agentAddress, new GrpcChannelOptions { HttpHandler = handler });
            agentJob = new JobA.JobAClient(agentChannelJob);


        CHOOSE:
            Console.WriteLine("Choose a option - ");
            Console.WriteLine("Start Job - 1");
            Console.WriteLine("Exit  - 0");

            var inputStr = Console.ReadLine();
            int input = -1;
            if (int.TryParse(inputStr, out input))
            {
                if (input == 0)
                    goto EXIT;

                if (input == 1)
                {
             
                    var resut =StartJob().Result;             
                    if (resut == true)
                    {
                        Console.WriteLine("Job started!!");
                        Console.ReadLine();
                        Console.WriteLine(Environment.NewLine);
                        goto CHOOSE;
                    }
                    else
                    {
                        Console.WriteLine("Error is starting job");
                        Console.ReadLine();
                        Console.WriteLine(Environment.NewLine);
                        goto CHOOSE;
                    }
                }
            }
            else
            {
                Console.WriteLine("Incorrect option, please enter valid value." + Environment.NewLine);
                goto CHOOSE;
            }

        EXIT:
            Console.WriteLine("Exiting application");
        }

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

        static async Task<bool> StartJob()
        {
            Console.WriteLine("Please enter job number:");
            var jobName = Console.ReadLine();

            Console.WriteLine("Please provide folder path:");
            var folderpath = Console.ReadLine();

            if (folderpath == null || !Directory.Exists(folderpath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while starting job - Invalid folder path!" + Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            try
            {
                var clientJobMsg = new ClientService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "", FolderPath = folderpath };
                var files = new List<string>();
                foreach (var file in Directory.GetFiles(folderpath))
                {
                    files.Add(file);
                }
                clientJobMsg.Files.AddRange(files);
                var isstartedClient = await clientJob.StartJobAsync(clientJobMsg);

                var agentJobMsg = new AgentService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "" };
                agentJobMsg.Files.AddRange(files);
                var isstartedAgent = await agentJob.StartJobAsync(agentJobMsg);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while starting job - ");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            return true;
        }



    }
}

