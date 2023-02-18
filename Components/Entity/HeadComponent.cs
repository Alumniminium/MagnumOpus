using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
    public struct HeadComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        private ushort hair;
        public ushort FaceId;

        internal ushort Hair
        {
            get => hair; set
            {
                hair = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, hair, MsgUserAttribType.HairStyle);
                entity.NetSync(ref packet, true);
            }
        }

        public HeadComponent(int entityId, ushort face=6, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Hair = hair;
            FaceId = face;
        }

        public override int GetHashCode() => EntityId;
    }
}