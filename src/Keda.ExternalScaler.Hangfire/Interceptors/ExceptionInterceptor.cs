using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace HangfireExternalScaler.Interceptors
{
    public class ExceptionInterceptor: Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (ArgumentException ex)
            {
                // Potentially log or metric this here
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                // Potentially log or metric this here
                throw new RpcException(new Status(StatusCode.Internal, ex.ToString()));
            }
        }
    }
}