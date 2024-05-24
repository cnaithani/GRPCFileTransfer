using AgentService.Modules.Interfaces;

namespace AgentService.Modules.Classes
{
    public class AgentJobDetails :IAgentJobDetails
    {
        public string JobNumber { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new List<string>();
        public string Status { get; set; } = string.Empty;
    }
}
