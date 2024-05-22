using AgentService.Modules.Interfaces;
using AgentService.Protos;

namespace AgentService.Modules.Classes
{
    public class AgentJobs : IAgentJobs
    {
        public IList<IAgentJobDetails> Jobs { get ; set; } = new List<IAgentJobDetails>();
       
        public bool StartJob(string jobNumber)
        {
            var jobDetail = new AgentJobDetails();
            jobDetail.JobNumber = jobNumber;

            Jobs.Add(jobDetail);

            return true;
        }
    }
}
