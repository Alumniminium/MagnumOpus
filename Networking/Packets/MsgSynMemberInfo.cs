using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

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

        public static byte[] Create(PixelEntity member)
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

        public static implicit operator byte[](MsgSynMemberInfo msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgSynMemberInfo*)p = *&msg;
            return buffer;
        }
    }
}