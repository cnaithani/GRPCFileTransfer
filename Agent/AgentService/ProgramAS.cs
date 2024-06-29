using AgentService.ExceptionHandling.Interceptor;
using AgentService.Modules.Classes;
using AgentService.Modules.Interfaces;
using AgentService.Protos;
using AgentService.Services;
using ClientService.Protos;
using Grpc.Net.Client;
using Serilog;

namespace AgentApp
{

    public class Program
    {

        static string AppName = "GRPC - Agent Service";
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
            builder.Services.AddGrpc(options =>
                options.Interceptors.Add<ErrorHandlingInterceptor>());

            builder.Services.AddSingleton(Log.Logger);
            builder.Services.AddSingleton<IAgentJobs, AgentJobs>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<JobService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            Task.Run(() => { app.Run(); });
            Thread.Sleep(1000);

            /*-----------------*/

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var agentAddress = config["Services:Agent"];
            var agentChannelJob = GrpcChannel.ForAddress(agentAddress, new GrpcChannelOptions { HttpHandler = handler });
            agentJob = new JobA.JobAClient(agentChannelJob);

        CHOOSE:
            Console.WriteLine("Choose a option - ");
            Console.WriteLine("Get All Jobs - 1");
            Console.WriteLine("Configure Job - 2");
            Console.WriteLine("Start Transfer - 3");
            Console.WriteLine("Exit  - 0");

            var inputStr = Console.ReadLine();
            int input = -1;
            if (int.TryParse(inputStr, out input))
            {
                if (input == 1)
                {
                    GetAllJobs();
                    Console.WriteLine(Environment.NewLine);
                    goto CHOOSE;
                }
                if (input == 2)
                {
                    Configure();
                    Console.WriteLine(Environment.NewLine);
                    goto CHOOSE;
                }
                if (input == 3)
                {
                    Transfer();
                    Console.WriteLine(Environment.NewLine);
                    goto CHOOSE;
                }
                else if (input == 0)
                {
                    goto EXIT;
                }
            }
            else
            {
                Console.WriteLine("Incorrect option, please enter valid value." + Environment.NewLine);
                goto CHOOSE;
            }

            Console.Read();

        EXIT:

            Console.WriteLine("Exiting");

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

        static void GetAllJobs()
        {
            var jobs = agentJob.GetJobs(new EmptyInput());

            if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
            {
                Console.WriteLine("No jobs are running in agent");
                return;
            }

            Console.WriteLine("Following jobs are running in agent - ");
            foreach (var job in jobs.Jobs)
            {
                Console.WriteLine("Job: " + job.Job + "    Status: " + job.Status);
            }

        }

        static void Configure()
        {
            var jobs = agentJob.GetJobs(new EmptyInput());

            if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
            {
                Console.WriteLine("No jobs are running in agent");
                return;
            }
            var jobList = jobs.Jobs.Where(x => x.Status == GSRCCommons.Constants.JOB_STATUS_STARTED).ToList();
            if (jobList.Count == 0)
            {
                Console.WriteLine("No jobs are running in agent");
                return;
            }
            Console.WriteLine("Following non-configured jobs are running in agent - ");
            foreach (var job in jobList)
            {
                Console.WriteLine("Job: " + job.Job);
            }
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Please enter job number (Case Sensitive) to configure  - ");

            var jobToConfigure = Console.ReadLine().Trim();
            if (!jobList.Select(x => x.Job).Contains(jobToConfigure))
            {
                Console.WriteLine("No jobs found with this name!");
                return;
            }

            Console.WriteLine("Please enter folder path to copy files - ");
            var folderpath = Console.ReadLine().Trim();
            var configureReply = agentJob.ConfigureJob(new ConfigureJobInput { Job = jobToConfigure, FolderPath = folderpath });
            if (configureReply.IsConfigured == true)
            {
                Console.WriteLine("Job configured successfully!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while configuring job ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine(Environment.NewLine);

        }

        static void Transfer()
        {
            var jobs = agentJob.GetJobs(new EmptyInput());

            if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
            {
                Console.WriteLine("No jobs are running in agent");
                return;
            }
            var jobList = jobs.Jobs.Where(x => x.Status == GSRCCommons.Constants.JOB_STATUS_READY).ToList();
            if (jobList.Count == 0)
            {
                Console.WriteLine("No jobs available for transferring");
                return;
            }
            Console.WriteLine("Following jobs are running in agent ready for transfer - ");
            foreach (var job in jobList)
            {
                Console.WriteLine("Job: " + job.Job);
            }
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Please enter job number (Case Sensitive) to transfer  - ");

            var jobToTransfer = Console.ReadLine().Trim();
            if (!jobList.Select(x => x.Job).Contains(jobToTransfer))
            {
                Console.WriteLine("No jobs found with this name!");
                return;
            }

            var trasferReply = agentJob.Transfer(new TransferInput { Job = jobToTransfer });
            if (trasferReply.HasTransferStarted)
            {
                Console.WriteLine("Done!");
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error while transferring! ");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

        }


    }


}
