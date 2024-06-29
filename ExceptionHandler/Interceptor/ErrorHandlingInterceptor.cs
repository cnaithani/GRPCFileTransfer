
using Grpc.Core;
using Grpc.Core.Interceptors;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ExceptionHandler;

namespace AgentService.ExceptionHandling.Interceptor
{
    public class ErrorHandlingInterceptor: Grpc.Core.Interceptors.Interceptor
    {
        public static Action<Exception> ExceptionLogger { get; set; }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, 
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            TResponse response = null;

            try
            {
                response = await continuation(request, context);
            }
            catch (ExceptionHandler.Exceptions.ValidtionException ex)
            {
                ExceptionLogger?.Invoke(ex);
                Metadata metadata = new Metadata();
                bool success = ExceptionHandler.ExceptionHandler.SetException(metadata, ex);

                if (success)
                {
                    throw new RpcException(new Status(StatusCode.Internal,
                        ExceptionHandler.ExceptionHandler.METADATA_KEY), metadata);
                }
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            return response;
           
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



    }
}
