using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public Direction Direction;
        private uint look = 1003;
        public uint Look
        {
            get => look;
            set
            {
                look = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, look, MsgUserAttribType.Look);
                ntt.NetSync(ref packet, true);
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
