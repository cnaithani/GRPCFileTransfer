using ClientService.Modules.Interfaces;
using ClientService.Protos;

namespace ClientService.Modules.Classes
{
    public class ClientJobs : IClientJobs
    {
        public IList<IClientJobDetails> Jobs { get ; set; } = new List<IClientJobDetails>();
       
        public bool StartJob(string jobNumber)
        {
            var jobDetail = new ClientJobDetails();
            jobDetail.JobNumber = jobNumber;

            Jobs.Add(jobDetail);

            return true;
        }
    }
}
