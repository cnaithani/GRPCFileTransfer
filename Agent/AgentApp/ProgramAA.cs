using AgentService.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System.Linq;

Console.WriteLine("Welcome to Agent App");

var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

var agentAddress = config["Services:Agent"];  
var agentChannelJob = GrpcChannel.ForAddress(agentAddress);
var agentJob = new JobA.JobAClient(agentChannelJob);


CHOOSE:
Console.WriteLine("Choose a option - ");
Console.WriteLine("Get All Jobs - 1");
Console.WriteLine("Configure Job - 2");
Console.WriteLine("Start Transfer - 3");
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
    if (input == 2)
    {
        Configure();
        Console.WriteLine(Environment.NewLine);
        goto CHOOSE;
    }
    if (input == 3)
    {
        Transfer();
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

    if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
    {
        Console.WriteLine("No jobs are running in agent");
        return;
    }

    Console.WriteLine("Following jobs are running in agent - ");
    foreach (var job in jobs.Jobs)
    {
        Console.WriteLine("Job: " + job.Job + "    Status: " + job.Status);
    }

}

void Configure()
{
    var jobs = agentJob.GetJobs(new EmptyInput());

    if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
    {
        Console.WriteLine("No jobs are running in agent");
        return;
    }
    var jobList = jobs.Jobs.Where(x => x.Status == GSRCCommons.Constants.JOB_STATUS_STARTED).ToList();
    if (jobList.Count == 0)
    {
        Console.WriteLine("No jobs are running in agent");
        return;
    }
    Console.WriteLine("Following non-configured jobs are running in agent - ");
    foreach (var job in jobList)
    {
        Console.WriteLine("Job: " + job.Job);
    }
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Please enter job number (Case Sensitive) to configure  - ");

    var jobToConfigure = Console.ReadLine().Trim();
    if (!jobList.Select(x => x.Job).Contains(jobToConfigure))
    {
        Console.WriteLine("No jobs found with this name!");
        return;
    }

    Console.WriteLine("Please enter folder path to copy files - ");
    var folderpath = Console.ReadLine().Trim();
    var configureReply = agentJob.ConfigureJob(new ConfigureJobInput { Job = jobToConfigure, FolderPath = folderpath });
    if (configureReply.IsConfigured == true)
    {
        Console.WriteLine("Job configured successfully!");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error while configuring job ");
        Console.ForegroundColor = ConsoleColor.White;
    }
    Console.WriteLine(Environment.NewLine);

}

void Transfer()
{
    var jobs = agentJob.GetJobs(new EmptyInput());

    if (jobs == null || jobs.Jobs == null || jobs.Jobs.Count == 0)
    {
        Console.WriteLine("No jobs are running in agent");
        return;
    }
    var jobList = jobs.Jobs.Where(x => x.Status == GSRCCommons.Constants.JOB_STATUS_READY).ToList();
    if (jobList.Count == 0)
    {
        Console.WriteLine("No jobs available for transferring");
        return;
    }
    Console.WriteLine("Following jobs are running in agent ready for transfer - ");
    foreach (var job in jobList)
    {
        Console.WriteLine("Job: " + job.Job);
    }
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Please enter job number (Case Sensitive) to transfer  - ");

    var jobToTransfer = Console.ReadLine().Trim();
    if (!jobList.Select(x => x.Job).Contains(jobToTransfer))
    {
        Console.WriteLine("No jobs found with this name!");
        return;
    }

    var trasferReply =agentJob.Transfer(new TransferInput { Job = jobToTransfer });
    if (trasferReply.HasTransferStarted)
    {
        Console.WriteLine("Done!");
        return;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error while transferring! ");
        Console.ForegroundColor = ConsoleColor.White;
        return;
    }

}

EXIT:

Console.WriteLine("Exiting");
