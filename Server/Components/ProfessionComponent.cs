using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ProfessionComponent
    {
        public NTT NTT;
        private ClasseName profession;

        public ClasseName Profession
        {
            get => profession; set
            {
                profession = value;
                var packet = MsgUserAttrib.Create(NTT, (uint)value, MsgUserAttribType.Class);
                NTT.NetSync(ref packet, true);
            }
        }

        public ProfessionComponent(in NTT ntt, ClasseName profession)
        {
            NTT = ntt;
            Profession = profession;
        }

        public override int GetHashCode() => NTT.Id;
    }
}