using ClientService.Protos;
using Grpc.Net.Client;

var clientAddress = "http://localhost:5034";
var channelJob = GrpcChannel.ForAddress(clientAddress);
var clientJob = new JobC.JobCClient(channelJob);

var job = new StartJobRequest { JobNumber = "123", MachineName = "" };

var res = await clientJob.StartJobAsync(job);

Console.ReadLine(); 