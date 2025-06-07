using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct RebornComponent(byte count = 0)
    {
        public long ChangedTick = NttWorld.Tick;
        public byte Count = count;
    }
}