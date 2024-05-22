using AgentService.Modules.Interfaces;

namespace AgentService.Modules.Interfaces
{
    public interface IAgentJobs
    {
        IList<IAgentJobDetails> Jobs { get; set; }
        bool StartJob(string jobNumber);
    }
}
