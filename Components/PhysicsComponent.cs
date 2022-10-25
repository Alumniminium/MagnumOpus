using System.Numerics;
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
        public Direction Direction;
        internal Emote Emote;

        public BodyComponent(int entityId, Direction direction = Direction.South, uint look = 1003)
        {
            EntityId = entityId;
            Direction = direction;
            ChangedTick = Game.CurrentTick;
            Look = look;
            Hair = 0;
            Emote = Emote.Stand;
        }


        public override int GetHashCode() => EntityId;
    }
}