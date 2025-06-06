using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTeamUpdate
    {
        public ushort Size;
        public ushort Id;
        public MsgTeamMemberAction Action;
        public byte Amount;
        public short NameLength;
        public fixed byte Name[16];
        public int UniqueId;
        public uint Look;
        public ushort MaxHp;
        public ushort CurHp;

        public static MsgTeamUpdate JoinLeave(in NTT owner, MsgTeamMemberAction action)
        {
            ref readonly var ntc = ref owner.Get<NameTagComponent>();
            ref readonly var bdy = ref owner.Get<BodyComponent>();
            ref readonly var hlt = ref owner.Get<HealthComponent>();

            var look = bdy.Look;

            var packet = new MsgTeamUpdate
            {
                Size = (ushort)sizeof(MsgTeamUpdate),
                Id = 1026,
                Action = action,
                Amount = 1,
                NameLength = 16,
                UniqueId = owner.Id,
                Look = look,
                MaxHp = (ushort)hlt.MaxHealth,
                CurHp = (ushort)hlt.Health
            };
            for (byte i = 0; i < ntc.Name.Length; i++)
                packet.Name[i] = (byte)ntc.Name[i];

            return packet;
        }
    }
}