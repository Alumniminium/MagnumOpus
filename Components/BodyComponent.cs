using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        
        internal ushort Hair;
        public uint Look;
        internal Emote Emote;
        public ushort FaceId;

        public BodyComponent(int entityId, uint look = 1003, Emote emote = Emote.Stand, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Look = look;
            Emote = emote;
            Hair = hair;
        }


        public override int GetHashCode() => EntityId;
    }
}