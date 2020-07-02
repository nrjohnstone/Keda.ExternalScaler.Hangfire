using System;
using System.Collections.Generic;
using Scaler;
using Serilog;

namespace Keda.ExternalScaler.Hangfire
{
    /// <summary>
    /// In memory implementation to store Hangfire external scaler configurations created by KEDA
    /// calling the New endpoint.
    /// If high availability for the Keda.ExternalScaler.Hangfire was a requirement, you would need to
    /// create an implementation store this out of proc (Redis, SQL server table etc..)
    /// and have all instances reading from the same store
    /// </summary>
    public class HangfireScaledObjectRepositoryInMemory : IHangfireScaledObjectRepository
    {
        private readonly Dictionary<string, HangfireScalerConfiguration> _data;

        public HangfireScaledObjectRepositoryInMemory()
        {
            _data = new Dictionary<string, HangfireScalerConfiguration>();
        }

        public void Store(NewRequest newRequest)
        {
            HangfireScalerConfiguration configuration = new HangfireScalerConfiguration()
            {
                Queue = newRequest.Metadata["queue"],
                MaxScale = Convert.ToInt64(newRequest.Metadata["maxScale"]),
                InstanceName = newRequest.Metadata["hangfireInstance"]
            };

            var key = $"{newRequest.ScaledObjectRef.Name}:{newRequest.ScaledObjectRef.Namespace}";

            Log.Debug("Created new configuration {Key} {InstanceName} {Queue} {MaxScale}",
                key, configuration.InstanceName, configuration.Queue, configuration.MaxScale);

            _data[key] = configuration;
        }

        public HangfireScalerConfiguration Get(ScaledObjectRef scaledObjectRef)
        {
            var key = $"{scaledObjectRef.Name}:{scaledObjectRef.Namespace}";
            Log.Debug("Retrieving configuration {Key}", key);
            return _data[key];
        }
    }
}