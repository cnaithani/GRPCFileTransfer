
using ClientService.Protos;
using Grpc.Net.Client;
using System.Runtime;

Console.WriteLine("Welcome to Client App");

var clientAddress = "http://localhost:5034";
var channelJob = GrpcChannel.ForAddress(clientAddress);
var clientJob = new JobC.JobCClient(channelJob);

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
        var job = new StartJobRequest { JobNumber = jobName, MachineName = "" };


        var isstarted = await clientJob.StartJobAsync(job);
        //var isstarted = clientJob.StartJob(job);
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

