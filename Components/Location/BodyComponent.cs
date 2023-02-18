using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public Direction Direction;
        private uint look;
        public uint Look
        {
            get => look; 
            set
            {
                look = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, look, MsgUserAttribType.Look);
                entity.NetSync(ref packet, true);
            }
        }


        public BodyComponent(int entityId, uint look = 1003, Direction direction = Direction.South)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Look = look;
            Direction = direction;
        }

        public override int GetHashCode() => EntityId;
    }
}
