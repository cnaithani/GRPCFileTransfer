using ClientService.Modules.Classes;
using ClientService.Modules.Interfaces;
using ClientService.Protos;
using Grpc.Core;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ClientService.Services
{
    public class JobService : JobC.JobCBase
    {
        public JobService(IClientJobs clientJobs, ILogger logger)
        {
            ClientJobs = clientJobs;
            Logger = logger;
        }
        IClientJobs ClientJobs { get; set; }
        Serilog.ILogger Logger { get; set; }

        public override async Task<StartJobReply> StartJob(StartJobRequest request, ServerCallContext context)
        {
            try
            {
                var isStarted = ClientJobs.StartJob(request.JobNumber);
                Logger.Information("Job srarted - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error starting job - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };
            }
;
        }
    }
}
