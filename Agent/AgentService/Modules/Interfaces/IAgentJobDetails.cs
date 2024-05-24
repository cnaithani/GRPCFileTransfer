namespace AgentService.Modules.Interfaces
{
    public interface IAgentJobDetails
    {
        string JobNumber { get; set; }
        string FolderPath { get; set; }
        string Status { get; set; }
    }
}
