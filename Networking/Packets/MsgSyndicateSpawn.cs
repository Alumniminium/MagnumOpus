using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components.Guild;
using MagnumOpus.Components.Entity;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgSyndicateSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Donation;
        public int Funds;
        public int MemberCount;
        public GuildRanks Rank;
        public fixed byte LeaderName[16];

        public static Memory<byte> Create(in NTT ntt)
        {
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var leader = ref NttWorld.GetEntity(gld.LeaderId);
            ref readonly var ntc = ref leader.Get<NameTagComponent>();

            var msgP = stackalloc MsgSyndicateSpawn[1];

            msgP->Size = (ushort)sizeof(MsgSyndicateSpawn);
            msgP->Id = 1106;

            msgP->UniqueId = gld.GuildId;
            msgP->Donation = gld.Donation;
            msgP->Funds = gld.Funds;
            msgP->Rank = gld.Rank;
            msgP->MemberCount = gld.Members.Length;

            for (byte i = 0; i < ntc.Name.Length; i++)
                msgP->LeaderName[i] = (byte)ntc.Name[i];

            var buffer = new byte[sizeof(MsgSyndicateSpawn)];
            fixed (byte* p = buffer)
                *(MsgSyndicateSpawn*)p = *msgP;
            return buffer;
        }
    }
}