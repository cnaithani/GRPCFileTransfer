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
                var isStarted = AgentJobs.StartJob(request.JobNumber, request.Files.ToList());
                Logger.Information("Job srarted - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error starting job - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };
            }
        }

        public override async Task<JobListModel> GetJobs(EmptyInput request, ServerCallContext context)
        {
            var jobs = new JobListModel();
            foreach (var x in AgentJobs.Jobs)
            {
                var job = new JobModel();
                job.Job = x.JobNumber;
                job.Status = x.Status;
                jobs.Jobs.Add(job);
            }
            return jobs;
        }

        public override async Task<ConfigureJobReply> ConfigureJob(ConfigureJobInput request, ServerCallContext context)
        {
            var job = AgentJobs.Jobs.Where(x => x.JobNumber == request.Job).FirstOrDefault();
            if (job == null)
                return new ConfigureJobReply { IsConfigured = false };

            job.FolderPath= request.FolderPath;
            job.Status = GSRCCommons.Constants.JOB_STATUS_READY;

            return new ConfigureJobReply { IsConfigured = true };
        }
    }
}
