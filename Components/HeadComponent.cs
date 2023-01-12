using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct HeadComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        internal ushort Hair;
        public ushort FaceId;

        public HeadComponent(int entityId, ushort face=6, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Hair = hair;
            FaceId = face;
        }

        public override int GetHashCode() => EntityId;
    }
}