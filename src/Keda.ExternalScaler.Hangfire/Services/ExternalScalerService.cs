using System;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Scaler;
using Serilog;

namespace Keda.ExternalScaler.Hangfire.Services
{
    public class ExternalScalerService : Scaler.ExternalScaler.ExternalScalerBase
    {
        private readonly IHangfireScaledObjectRepository _hangfireScaledObjectRepository;

        private readonly IHangfireMetricsApi _hangfireMetricsApi;
        
        public ExternalScalerService(IHangfireScaledObjectRepository hangfireScaledObjectRepository,
            IHangfireMetricsApi hangfireMetricsApi)
        {
            if (hangfireScaledObjectRepository == null)
                throw new ArgumentNullException(nameof(hangfireScaledObjectRepository));
            if (hangfireMetricsApi == null) 
                throw new ArgumentNullException(nameof(hangfireMetricsApi));

            _hangfireScaledObjectRepository = hangfireScaledObjectRepository;
            _hangfireMetricsApi = hangfireMetricsApi;
        }

        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            Log.Information("New: Entry()");
            try
            {
                _hangfireScaledObjectRepository.Store(request);
                return Task.FromResult(new Empty());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in New()");
                throw;
            }
        }

        public override Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            Log.Information("IsActive");

            try
            {
                var scalerConfiguration = _hangfireScaledObjectRepository.Get(request);
                Log.Information("ScalerConfiguration retrieved: {HangfireInstanceName} {Queue}",
                    scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                long enqueuedCount = _hangfireMetricsApi.EnqueuedCount(
                    scalerConfiguration.InstanceName, scalerConfiguration.Queue);
                long fetchedCount = _hangfireMetricsApi.FetchedCount(
                    scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                if (enqueuedCount > 0 || fetchedCount > 0)
                {
                    return Task.FromResult(new IsActiveResponse() {Result = true});
                }

                return Task.FromResult(new IsActiveResponse() {Result = false});
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in IsActive()");
                throw;
            }
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            Log.Information("GetMetricSpec");
            try
            {
                var scalerConfiguration = _hangfireScaledObjectRepository.Get(request);

                var response = new GetMetricSpecResponse();
                var fields = new RepeatedField<MetricSpec>();

                fields.Add(new MetricSpec()
                {
                    MetricName = "ScaleRecommendation",
                    TargetSize = scalerConfiguration.MaxScale
                });

                response.MetricSpecs.Add(fields);

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in GetMetricSpec()");
                throw;
            }
        }

        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            Log.Information("GetMetrics");
            try
            {
                if (request.MetricName.Equals("queueLength"))
                {
                    var scalerConfiguration = _hangfireScaledObjectRepository.Get(request.ScaledObjectRef);
                    Log.Information("ScalerConfiguration retrieved: {HangfireInstanceName} {Queue}", 
                        scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                    long enqueuedCount = _hangfireMetricsApi.EnqueuedCount(
                        scalerConfiguration.InstanceName, scalerConfiguration.Queue);

                    Log.Information("Enqueued Count: {EnqueuedCount}", enqueuedCount);

                    var response = new GetMetricsResponse();

                    response.MetricValues.Add(new MetricValue()
                    {
                        MetricName = "queueLength",
                        MetricValue_ = enqueuedCount
                    });
                    
                    return Task.FromResult(response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in GetMetrics()");
                throw;
            }

            Log.Error("MetricName {MetricName} is not implemented", request.MetricName);
            throw new NotSupportedException($"MetricName {request.MetricName} is not implemented");
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            // TODO : Close should maybe remove the configuration from the in memory repository
            return Task.FromResult(new Empty());
        }
    }
}