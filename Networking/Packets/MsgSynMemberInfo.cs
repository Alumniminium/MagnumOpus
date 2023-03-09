using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components.Guild;
using MagnumOpus.Components.Entity;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgSynMemberInfo
    {
        public ushort Size;
        public ushort Id;
        public int Donation;
        public GuildRanks Rank;
        public fixed byte Name[16];

        public static MsgSynMemberInfo Create(in NTT member)
        {
            ref readonly var gld = ref member.Get<GuildComponent>();
            ref readonly var ntc = ref member.Get<NameTagComponent>();

            var packet = new MsgSynMemberInfo
            {
                Size = (ushort)sizeof(MsgSynMemberInfo),
                Id = 1112,
                Donation = gld.Donation,
                Rank = gld.Rank,
            };
            for (byte i = 0; i < ntc.Name.Length; i++)
                packet.Name[i] = (byte)ntc.Name[i];
            return packet;
        }
    }
}