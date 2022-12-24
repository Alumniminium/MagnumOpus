using MagnumOpus.ECS;

namespace MagnumOpus.Components
{

    [Component]
    public struct LevelComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public byte Level;

        public LevelComponent(int entityId, byte level=1)
        {
            EntityId = entityId;
            Level = level;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}