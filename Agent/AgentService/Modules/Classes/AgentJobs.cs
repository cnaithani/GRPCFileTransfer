using AgentService.Models;
using AgentService.Modules.Interfaces;
using AgentService.Protos;
using ClientService.Protos;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog.Core;
using System.Runtime;
using System.Threading.Channels;

namespace AgentService.Modules.Classes
{
    public class AgentJobs : IAgentJobs
    {
        public AgentJobs(IConfiguration configuration) 
        {
            Config = configuration;
        }
        public IList<IAgentJobDetails> Jobs { get ; set; } = new List<IAgentJobDetails>();
        public IConfiguration Config { get; set; }
       
        public bool StartJob(string machineName, string jobNumber, List<string> files)
        {
            var jobDetail = new AgentJobDetails();
            jobDetail.MachineName = machineName;
            jobDetail.JobNumber = jobNumber;
            jobDetail.Status = GSRCCommons.Constants.JOB_STATUS_STARTED;       
            foreach (var file in files)
            {
                jobDetail.Files.Add(new FileModel { FilePath = file , IsComplete = false});
            }

            Jobs.Add(jobDetail);

            return true;
        }

        public async Task<bool> Transfer(string jobNumber)
        {
            var clientAddress = Config["Services:Client"];

            var channel = GrpcChannel.ForAddress(clientAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 5 * 1024 * 1024, // 5 MB
                MaxSendMessageSize = 5 * 1024 * 1024, // 5 MB
            });
            var client = new JobC.JobCClient(channel);

            var job = Jobs.Where(x => x.JobNumber == jobNumber).FirstOrDefault();

            foreach (var file in job.Files.Where(x => x.IsComplete == false).ToList())
            {
                var isSuccess = await GetFile(file.FilePath, job.FolderPath, client);
                if (isSuccess)
                {
                    file.IsComplete = true;
                }
            }

            if (job.Files.Where(x => x.IsComplete == false).ToList().Count() == 0)
            {
                job.Status = GSRCCommons.Constants.JOB_STATUS_DONE;
                return true;
            }
            else
            {
                return false;
            }
        }


        private async Task<bool> GetFile(string filePath, string folderPath, JobC.JobCClient client)
        {
            try
            {
                var _request = new FileRequest { FilePath = filePath };
                if (!folderPath.EndsWith(Path.DirectorySeparatorChar))
                    folderPath += Path.DirectorySeparatorChar;
                var _temp_file = Path.Combine( folderPath , $"temp{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.tmp");
                var _final_file = _temp_file;

                using (var _call = client.SendReciveFile(_request))
                {
                    await using (var _fs = File.OpenWrite(_temp_file))
                    {
                        await foreach (var _chunk in _call.ResponseStream.ReadAllAsync().ConfigureAwait(false))
                        {
                            var _total_size = _chunk.FileSize;

                            if (!String.IsNullOrEmpty(_chunk.FileName))
                            {
                                _final_file = _chunk.FileName;
                            }

                            if (_chunk.Chunk.Length == _chunk.ChunkSize)
                                _fs.Write(_chunk.Chunk.ToByteArray());
                            else
                            {
                                _fs.Write(_chunk.Chunk.ToByteArray(), 0, _chunk.ChunkSize);
                                Console.WriteLine($"final chunk size: {_chunk.ChunkSize}");
                            }
                        }
                    }
                }

                if (_final_file != _temp_file)
                    File.Move(_temp_file, Path.Combine( folderPath + $"{_final_file}"));

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
