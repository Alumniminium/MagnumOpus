using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct DeathTagComponent
    {
        public readonly NTT Killer;
        public readonly long Tick;

        [JsonConstructor]
        public DeathTagComponent(in NTT killer)
        {
            Killer = killer;
            Tick = NttWorld.Tick;
        }
    }
}