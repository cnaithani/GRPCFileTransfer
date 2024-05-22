
using AgentService.Protos;
using ClientService.Protos;
using Grpc.Net.Client;
using System.Runtime;

Console.WriteLine("Welcome to Client App");

var clientAddress = "http://localhost:5034";
var clientChannelJob = GrpcChannel.ForAddress(clientAddress);
var clientJob = new JobC.JobCClient(clientChannelJob);

var agentAddress = "http://localhost:5276";
var agentChannelJob = GrpcChannel.ForAddress(agentAddress);
var agentJob = new JobA.JobAClient(agentChannelJob);

CHOOSE:
Console.WriteLine("Choose a option - ");
Console.WriteLine("Start Job - 1");
Console.WriteLine("Exit  - 0");

var inputStr = Console.ReadLine();
int input = -1;
if  (int.TryParse(inputStr, out input))
{
    if (await StartJob() == true)
    {
        Console.WriteLine("Job started!!");
        Console.ReadLine();
        Console.WriteLine(Environment.NewLine);
        goto CHOOSE;
    }
    else
    {
        Console.WriteLine("Error is starting job");
        Console.ReadLine();
        Console.WriteLine(Environment.NewLine);
        goto CHOOSE;
    }
}
else
{
    Console.WriteLine("Incorrect option, please enter valid value." + Environment.NewLine);
    goto CHOOSE;
}

async Task<bool> StartJob()
{
    Console.WriteLine("Please enter job number");
    var jobName = Console.ReadLine();
    try
    {
        var clientJobMsg = new ClientService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "" };
        var isstartedClient = await clientJob.StartJobAsync(clientJobMsg);

        var agentJobMsg = new AgentService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "" };
        var isstartedAgent= await agentJob.StartJobAsync(agentJobMsg);

    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error while starting job - ");
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    return true;
}

