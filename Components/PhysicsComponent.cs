using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        
        internal ushort Hair;
        public uint Look;
        internal Emote Emote;

        public BodyComponent(int entityId, uint look = 1003, Emote emote = Emote.Stand, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = ConquerWorld.Tick;
            Look = look;
            Emote = emote;
            Hair = hair;
        }


        public override int GetHashCode() => EntityId;
    }
}