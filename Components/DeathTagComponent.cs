using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct DeathTagComponent
    {
        public NTT Killer;
        public long Tick;

        public DeathTagComponent() => Tick = NttWorld.Tick;
        public DeathTagComponent(in NTT killer)
        {
            Killer = killer;
            Tick = NttWorld.Tick;
        }
    }
}