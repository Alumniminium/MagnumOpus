using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SourceGeneration;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct AttributeComponent
    {
        public NTT NTT;
        public long ChangedTick = NttWorld.Tick;

        [NetworkSync(MsgUserAttribType.Strength)]
        public ushort Strength;

        [NetworkSync(MsgUserAttribType.Agility)]
        public ushort Agility;

        [NetworkSync(MsgUserAttribType.Vitality)]
        public ushort Vitality;

        [NetworkSync(MsgUserAttribType.Spirit)]
        public ushort Spirit;

        [NetworkSync(MsgUserAttribType.StatPoints)]
        public ushort StatPoints;

        public AttributeComponent(in NTT ntt)
        {
            NTT = ntt;
            Strength = 0;
            Agility = 0;
            Vitality = 0;
            Spirit = 0;
            StatPoints = 0;
        }

        public override int GetHashCode() => NTT.Id;
    }
}