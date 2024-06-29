using AgentService.Models;
using AgentService.Modules.Interfaces;

namespace AgentService.Modules.Classes
{
    public class AgentJobDetails :IAgentJobDetails
    {
        public string MachineName { get; set; } = string.Empty;
        public string JobNumber { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<FileModel> Files { get; set; } = new List<FileModel>();
        public string Status { get; set; } = string.Empty;
    }
}
