using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct ProfessionComponent
    {
        public readonly int EntityId;
        private ClasseName profession;

        public ClasseName Profession
        {
            get => profession; set
            {
                profession = value;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, (uint)value, MsgUserAttribType.Class);
                entity.NetSync(ref packet, true);
            }
        }

        public ProfessionComponent(int entityId, ClasseName profession)
        {
            EntityId = entityId;
            Profession = profession;
        }

        public override int GetHashCode() => EntityId;
    }
}