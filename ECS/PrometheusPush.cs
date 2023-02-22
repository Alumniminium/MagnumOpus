using Prometheus;

namespace MagnumOpus.ECS
{
    public static class PrometheusPush
    {
        public static readonly Gauge NTTCount = Metrics.CreateGauge("MAGNUMOPUS_ENTITIES", "Counter for Entities"); 
        public static readonly Counter TickCount = Metrics.CreateCounter("MAGNUMOPUS_TICKS", "Counter for Ticks"); 
    }
}