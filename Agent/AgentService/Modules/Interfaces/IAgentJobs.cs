using AgentService.Modules.Interfaces;
using System.Security.Cryptography.Xml;

namespace AgentService.Modules.Interfaces
{
    public interface IAgentJobs
    {
        IList<IAgentJobDetails> Jobs { get; set; }
        bool StartJob(string jobNumber, List<string> files);
        Task<bool> Transfer(string jobNumber);
    }
}
