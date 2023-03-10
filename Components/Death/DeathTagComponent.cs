using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct DeathTagComponent
    {
        public readonly NTT Killer;
        public readonly long Tick;

        public DeathTagComponent(in NTT killer)
        {
            Killer = killer;
            Tick = NttWorld.Tick;
        }
    }
}