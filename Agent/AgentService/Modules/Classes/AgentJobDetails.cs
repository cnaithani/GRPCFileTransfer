using AgentService.Modules.Interfaces;

namespace AgentService.Modules.Classes
{
    public class AgentJobDetails :IAgentJobDetails
    {
        public string JobNumber { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
    }
}
