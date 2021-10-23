using System;
using System.Threading.Tasks;
using Externalscaler;
using Google.Protobuf.Collections;
using Grpc.Core;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace HangfireExternalScaler.Services
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private readonly IHangfireMetricsApi _hangfireMetricsApi;
        
        public ExternalScalerService(IHangfireMetricsApi hangfireMetricsApi) : base()
        {
            if (hangfireMetricsApi == null) 
                throw new ArgumentNullException(nameof(hangfireMetricsApi));
            
            _hangfireMetricsApi = hangfireMetricsApi;
        }

        public override Task<IsActiveResponse> IsActive(ScaledObjectRef scaledObjectRef, ServerCallContext context)
        {
            using (LogContext.PushProperty("Method", nameof(IsActive)))
            {
                try
                {
                    Log.Debug($"Entry: {nameof(IsActive)}");

                    HangfireScalerConfiguration scalerConfiguration = scaledObjectRef.GetHangfireScalerConfiguration();

                    ILogEventEnricher[] properties = new ILogEventEnricher[]
                    {
                        new PropertyEnricher("InstanceName", scalerConfiguration.InstanceName),
                        new PropertyEnricher("Queue", scalerConfiguration.Queue),
                    };

                    using (LogContext.Push(properties))
                    {
                        ValidateHangfireInstanceIsConfigured(scalerConfiguration);

                        long enqueuedCount = _hangfireMetricsApi.EnqueuedCount(scalerConfiguration.InstanceName,
                            scalerConfiguration.Queue);

                        long fetchedCount = _hangfireMetricsApi.FetchedCount(scalerConfiguration.InstanceName,
                            scalerConfiguration.Queue);

                        bool isActive = true;

                        // Only allow scaling to 0 when fetchedCount (jobs currently being processed)
                        // is also 0
                        if (enqueuedCount == 0 && fetchedCount == 0)
                        {
                            isActive = false;
                        }

                        Log.Debug("Enqueued/Fetched: {EnqueuedCount}/{FetchedCount}",
                            enqueuedCount, fetchedCount);
                        Log.Debug("IsActive: {IsActive}", isActive);

                        var isActiveResponse = new IsActiveResponse() { Result = isActive };
                        Log.Debug($"Exit: {nameof(IsActive)}");
                        return Task.FromResult(isActiveResponse);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception: {ExceptionType}", ex.GetType());
                    throw new RpcException(new Status(StatusCode.Internal, ex.Message));
                }
            }
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef scaledObjectRef, ServerCallContext context)
        {
            using (LogContext.PushProperty("Method", nameof(GetMetricSpec)))
            {
                try
                {
                    Log.Debug($"Entry: {nameof(GetMetricSpec)}");

                    Log.Information("GetMetricSpec: {Name}", scaledObjectRef.Name);
                    Log.Debug("ScaledObjectRef {@ScaledObjectRef}", scaledObjectRef);

                    var scalerConfiguration = scaledObjectRef.GetHangfireScalerConfiguration();
                    ValidateHangfireInstanceIsConfigured(scalerConfiguration);

                    var response = new GetMetricSpecResponse();
                    var fields = new RepeatedField<MetricSpec>();

                    fields.Add(new MetricSpec()
                    {
                        MetricName = "ScaleRecommendation",
                        TargetSize = scalerConfiguration.TargetSize
                    });

                    response.MetricSpecs.Add(fields);

                    Log.Debug($"Exit: {nameof(GetMetricSpec)}");
                    return Task.FromResult(response);
                }
                catch (ArgumentException ex)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception: {ExceptionType}", ex.GetType());
                    throw new RpcException(new Status(StatusCode.Internal, ex.Message));
                }
            }
        }

        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            ILogEventEnricher[] properties = new ILogEventEnricher[]
            {
                new PropertyEnricher("Method", nameof(GetMetrics)),
                new PropertyEnricher("MetricName", request.MetricName),
                new PropertyEnricher("ScaleObjectRefName", request.ScaledObjectRef.Name),
                new PropertyEnricher("ScaleObjectRefNamespace", request.ScaledObjectRef.Namespace),
            };
            
            using (LogContext.Push(properties))
            {
                try
                {
                    Log.Information("GetMetrics: {MetricName}", request.MetricName);
                    Log.Information("ScaledObjectRef {@ScaledObjectRef}", request.ScaledObjectRef);

                    HangfireScalerConfiguration scalerConfiguration =
                        request.ScaledObjectRef.GetHangfireScalerConfiguration();

                    ValidateHangfireInstanceIsConfigured(scalerConfiguration);

                    // TODO : ScaledJobs in KEDA always request queueLength, need to implement handling for ScaledObject
                    if (request.MetricName.Equals("queueLength"))
                    {
                        ILogEventEnricher[] scalerProperties = new ILogEventEnricher[]
                        {
                            new PropertyEnricher("InstanceName", scalerConfiguration.InstanceName),
                            new PropertyEnricher("Queue", scalerConfiguration.Queue),
                        };

                        using (LogContext.Push(scalerProperties))
                        {
                            Log.Debug("Retrieving metrics for {InstanceName} {Queue}",
                                scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                            long enqueuedCount = _hangfireMetricsApi.EnqueuedCount(
                                scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                            Log.Debug("Enqueued: {EnqueuedCount}", enqueuedCount);

                            var response = new GetMetricsResponse();

                            var queueLength = enqueuedCount;

                            response.MetricValues.Add(new MetricValue()
                            {
                                MetricName = "queueLength",
                                MetricValue_ = queueLength
                            });

                            Log.Debug("QueueLength: {QueueLength}", queueLength);

                            Log.Debug("GetMetrics:Exit");

                            return Task.FromResult(response);
                        }
                    }

                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"MetricName {request.MetricName} is not implemented"));
                }
                catch (ArgumentException ex)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception: {ExceptionType}", ex.GetType());
                    throw new RpcException(new Status(StatusCode.Internal, ex.Message));
                }

            }
        }
        
        private void ValidateHangfireInstanceIsConfigured(HangfireScalerConfiguration scalerConfiguration)
        {
            if (!_hangfireMetricsApi.Exists(scalerConfiguration.InstanceName))
            {
                Log.Error("Hangfire instance {HangfireInstanceName} is not configured",
                    scalerConfiguration.InstanceName);
                throw new ArgumentException(
                    $"Hangfire instance {scalerConfiguration.InstanceName} is not configured");
            }
        }
    }
}