using AgentService.Modules.Interfaces;
using AgentService.Protos;
using Grpc.Core;
using Serilog;
using ILogger = Serilog.ILogger;

namespace AgentService.Services
{
    public class JobService: JobA.JobABase
    {
        public JobService(IAgentJobs agentJobs, ILogger logger)
        {
            AgentJobs = agentJobs;
            Logger = logger;
        }
        IAgentJobs AgentJobs { get; set; }
        Serilog.ILogger Logger { get; set; }

        public override async Task<StartJobReply> StartJob(StartJobRequest request, ServerCallContext context)
        {
            try
            {
                var isStarted = AgentJobs.StartJob(request.JobNumber);
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
