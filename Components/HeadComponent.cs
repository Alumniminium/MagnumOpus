using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct HeadComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        internal ushort Hair;
        public ushort FaceId;

        public HeadComponent(int entityId, ushort face, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Hair = hair;
            FaceId = 6;
        }

        public override int GetHashCode() => EntityId;
    }
}