namespace ClientService.Modules.Interfaces
{
    public interface IClientJobs
    {
        IList<IClientJobDetails> Jobs { get; set; }
        bool StartJob(string jobNumber);
    }
}
