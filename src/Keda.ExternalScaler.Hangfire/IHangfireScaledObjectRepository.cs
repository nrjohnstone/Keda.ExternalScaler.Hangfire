using Scaler;

namespace Keda.ExternalScaler.Hangfire
{
    public interface IHangfireScaledObjectRepository
    {
        void Store(NewRequest newRequest);
        HangfireScalerConfiguration Get(ScaledObjectRef scaledObjectRef);
    }
}