
using AgentService.Protos;
using ClientService.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System.Runtime;

Console.WriteLine("Welcome to Client App");

var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();


var clientAddress = config["Services:Client"];
var clientChannelJob = GrpcChannel.ForAddress(clientAddress);
var clientJob = new JobC.JobCClient(clientChannelJob);

var agentAddress = config["Services:Agent"]; 
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
    if (input == 0)
        goto EXIT;
       
    if (input == 1)
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
}
else
{
    Console.WriteLine("Incorrect option, please enter valid value." + Environment.NewLine);
    goto CHOOSE;
}

async Task<bool> StartJob()
{
    Console.WriteLine("Please enter job number:");
    var jobName = Console.ReadLine();

    Console.WriteLine("Please provide folder path:");
    var folderpath = Console.ReadLine();

    if (folderpath == null || !Directory.Exists(folderpath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error while starting job - Invalid folder path!" + Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    try
    {
        var clientJobMsg = new ClientService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "" , FolderPath = folderpath };
        var files = new List<string>();
        foreach (var file in Directory.GetFiles(folderpath))
        {
            files.Add(file);
        }
        clientJobMsg.Files.AddRange(files);
        var isstartedClient = await clientJob.StartJobAsync(clientJobMsg);

        var agentJobMsg = new AgentService.Protos.StartJobRequest { JobNumber = jobName, MachineName = "" };
        agentJobMsg.Files.AddRange(files);
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

EXIT:
Console.WriteLine("Exiting application");

