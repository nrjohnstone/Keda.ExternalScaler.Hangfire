using System;
using Externalscaler;

namespace HangfireExternalScaler.Services
{
    public static class ScaledObjectRefExtensions
    {
        public static HangfireScalerConfiguration GetHangfireScalerConfiguration(this ScaledObjectRef scaledObjectRef)
        {
            if (!scaledObjectRef.ScalerMetadata.ContainsKey("hangfireInstance"))
            {
                throw new ArgumentException("hangfireInstance must be specified");
            }
            
            if (!scaledObjectRef.ScalerMetadata.ContainsKey("queue"))
            {
                throw new ArgumentException("queue must be specified");
            }
          
            if (!scaledObjectRef.ScalerMetadata.ContainsKey("targetSize"))
            {
                throw new ArgumentException("targetSize must be specified");
            }

            HangfireScalerConfiguration scalerConfiguration = new HangfireScalerConfiguration()
            {
                Queue = scaledObjectRef.ScalerMetadata["queue"],
                InstanceName = scaledObjectRef.ScalerMetadata["hangfireInstance"],
                TargetSize = Convert.ToInt32(scaledObjectRef.ScalerMetadata["targetSize"]),
            };

            return scalerConfiguration;
        }
    }
}