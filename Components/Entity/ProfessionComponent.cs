using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct ProfessionComponent
    {
        public readonly int EntityId;
        private ClasseName profession;

        public ClasseName Profession
        {
            get => profession; set
            {
                profession = value;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, (uint)value, MsgUserAttribType.Class);
                ntt.NetSync(ref packet, true);
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