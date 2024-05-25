using ClientService.Modules.Classes;
using ClientService.Modules.Interfaces;
using ClientService.Protos;
using Google.Protobuf;
using Grpc.Core;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ClientService.Services
{
    public class JobService : JobC.JobCBase
    {
        public JobService(IClientJobs clientJobs, ILogger logger)
        {
            ClientJobs = clientJobs;
            Logger = logger;
        }
        IClientJobs ClientJobs { get; set; }
        Serilog.ILogger Logger { get; set; }

        public override async Task<StartJobReply> StartJob(StartJobRequest request, ServerCallContext context)
        {
            try
            {
                var isStarted = ClientJobs.StartJob(request.JobNumber);
                Logger.Information("Job srarted - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error starting job - " + request.JobNumber);
                return new StartJobReply { IsStarted = "true" };
            }
;
        }

        public override async Task SendReciveFile(FileRequest request, IServerStreamWriter<ChunkMsg> responseStream, ServerCallContext context)
        {
            var _file_path = request.FilePath;

            if (File.Exists(_file_path))
            {
                var _file_info = new FileInfo(_file_path);

                var _chunk = new ChunkMsg
                {
                    FileName = Path.GetFileName(_file_path),
                    FileSize = _file_info.Length
                };

                var _chunk_size = 64 * 1024;

                var _file_bytes = File.ReadAllBytes(_file_path);
                var _file_chunk = new byte[_chunk_size];

                var _offset = 0;

                while (_offset < _file_bytes.Length)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                        break;

                    var _length = Math.Min(_chunk_size, _file_bytes.Length - _offset);
                    Buffer.BlockCopy(_file_bytes, _offset, _file_chunk, 0, _length);

                    _offset += _length;

                    _chunk.ChunkSize = _length;
                    _chunk.Chunk = ByteString.CopyFrom(_file_chunk);

                    await responseStream.WriteAsync(_chunk).ConfigureAwait(false);
                }
            }
        }


    }
}
