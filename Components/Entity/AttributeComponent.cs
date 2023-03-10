using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct AttributeComponent
    {
        public  NTT NTT;
        public long ChangedTick;
        private ushort strength;
        private ushort agility;
        private ushort vitality;
        private ushort spirit;
        private ushort statpoints;

        public ushort Strength
        {
            get => strength;
            set
            {
                strength = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, value, MsgUserAttribType.Strength);
                NTT.NetSync(ref packet, true);
            }
        }
        public ushort Agility
        {
            get => agility;
            set
            {
                agility = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, value, MsgUserAttribType.Agility);
                NTT.NetSync(ref packet, true);
            }
        }
        public ushort Vitality
        {
            get => vitality;
            set
            {
                vitality = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, value, MsgUserAttribType.Vitality);
                NTT.NetSync(ref packet, true);
            }
        }
        public ushort Spirit
        {
            get => spirit;
            set
            {
                spirit = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, value, MsgUserAttribType.Spirit);
                NTT.NetSync(ref packet, true);
            }
        }
        public ushort Statpoints
        {
            get => statpoints;
            set
            {
                statpoints = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, value, MsgUserAttribType.StatPoints);
                NTT.NetSync(ref packet, true);
            }
        }

        public AttributeComponent(in NTT ntt)
        {
            NTT = ntt;
            Strength = 0;
            Agility = 0;
            Vitality = 0;
            Spirit = 0;
            Statpoints = 0;
        }

        public override int GetHashCode() => NTT.Id;
    }
}