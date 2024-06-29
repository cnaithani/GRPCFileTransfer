using Grpc.Core;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Reflection;

namespace ExceptionHandler
{
    public static class ExceptionHandler
    {
        //the case is always lowered when receiving metadata
        public const string METADATA_KEY = "capturedexception";
        public static bool SetException(Metadata metadata, Exception exception)
        {
            Type actualType = exception.GetType();
            IExceptionSerializer exceptionSerializer = GetSerializer(actualType);
            if (exceptionSerializer != null)
            {
                //serialize exception
                MemoryStream memoryStream = new MemoryStream();
                exceptionSerializer.Serialize(exception, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                byte[] byteContent = memoryStream.ToArray();

                //create error model
                CapturedExceptionMeta exceptionContent = new CapturedExceptionMeta(actualType, byteContent);
                string serializedExceptionContent = JsonConvert.SerializeObject(exceptionContent);
                metadata.Add(new Metadata.Entry(METADATA_KEY, serializedExceptionContent));
                return true;
            }
            return false;
        }
        public static Exception TryGetException(Exception exception)
        {
            if (exception is RpcException rpcException)
            {
                if (rpcException.Trailers != null
                && rpcException.StatusCode == StatusCode.Internal
                && string.Equals(rpcException.Status.Detail, ExceptionHandler.METADATA_KEY, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return ExceptionHandler.TryGetException(rpcException.Trailers);
                }
            }
            else
            {
                if (exception.InnerException != null)
                {
                    return TryGetException(exception.InnerException);
                }
            }
            return null;
        }
        private static IExceptionSerializer GetSerializer(Type actualType)
        {
            IExceptionSerializer exceptionSerializer = null;
            if (actualType.GetCustomAttribute<ExceptionSerializerAttribute>() is ExceptionSerializerAttribute serializer)
            {
                exceptionSerializer = (IExceptionSerializer)Activator.CreateInstance(serializer.SerializerType);
            }
            return exceptionSerializer;
        }
        private static Exception TryGetException(Metadata metadata)
        {
            if (metadata.Any(x => string.Equals(x.Key, METADATA_KEY, StringComparison.OrdinalIgnoreCase)))
            {
                //get message
                string serializedExceptionContent = metadata.SingleOrDefault(x => string.Equals(x.Key, METADATA_KEY, StringComparison.OrdinalIgnoreCase))?.Value;
                if (serializedExceptionContent == null)
                {
                    return new ArgumentNullException($"Metadata key with value \'{ExceptionHandler.METADATA_KEY}\' found in the metadata headers but value seems to be null. Could not deserialize into " +
                        $"\'{nameof(CapturedExceptionMeta)}\'");
                }

                CapturedExceptionMeta exceptionContent = JsonConvert.DeserializeObject<CapturedExceptionMeta>(serializedExceptionContent);

                //read byte content
                MemoryStream memoryStream = new MemoryStream(exceptionContent.Serialized);
                memoryStream.Seek(0, SeekOrigin.Begin);
                //get proper deserializer
                IExceptionSerializer exceptionSerializer = GetSerializer(exceptionContent.ExceptionType);
                if (exceptionSerializer == null)
                {
                    throw new Exception($"Could not handle error content deserialization. Serializer for type {exceptionContent.ExceptionType} not found. Make sure the exception class is " +
                        $"decorated with the attribute {nameof(ExceptionSerializerAttribute)}");
                }
                return exceptionSerializer.Deserialize(memoryStream);
            }
            return null;
        }
    }

}
