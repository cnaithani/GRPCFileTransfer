using AgentService.Protos;
using Grpc.Net.Client;

Console.WriteLine("Welcome to Agent App");

var agentAddress = "http://localhost:5276";
var agentChannelJob = GrpcChannel.ForAddress(agentAddress);
var agentJob = new JobA.JobAClient(agentChannelJob);

//GetAllJobs();

CHOOSE:
Console.WriteLine("Choose a option - ");
Console.WriteLine("Get All Jobs - 1");
Console.WriteLine("Exit  - 0");

var inputStr = Console.ReadLine();
int input = -1;
if (int.TryParse(inputStr, out input))
{
    if (input == 1)
    {
        GetAllJobs();
        Console.WriteLine(Environment.NewLine);
        goto CHOOSE;
    }
    else if (input == 0)
    {
        goto EXIT;
    }
}
else
{
    Console.WriteLine("Incorrect option, please enter valid value." + Environment.NewLine);
    goto CHOOSE;
}

Console.Read();

void GetAllJobs()
{
    var jobs = agentJob.GetJobs(new EmptyInput());

    if (jobs== null || jobs.Jobs == null || jobs.Jobs.Count == 0)
    {
        Console.WriteLine("No jobs are running in agent");
        return;
    }

    Console.WriteLine("Following jobs are running in agent - ");
    foreach(var job in jobs.Jobs)
    {
        Console.WriteLine("Job: " + job.Job + "    Status: " + job.Status);
    }

}

EXIT:

Console.WriteLine("Exiting");
