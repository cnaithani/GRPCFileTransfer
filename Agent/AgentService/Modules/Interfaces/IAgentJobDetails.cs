using AgentService.Models;

namespace AgentService.Modules.Interfaces
{
    public interface IAgentJobDetails
    {
        string MachineName { get; set; } 
        string JobNumber { get; set; }
        string FolderPath { get; set; }
        List<FileModel> Files { get; set; }
        string Status { get; set; }
    }
}
