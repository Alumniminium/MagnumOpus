using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
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
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, hair, MsgUserAttribType.HairStyle);
                ntt.NetSync(ref packet, true);
            }
        }

        public HeadComponent(int entityId, ushort face = 6, ushort hair = 310)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Hair = hair;
            FaceId = face;
        }

        public override int GetHashCode() => EntityId;
    }
}