using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 101)]
    public unsafe struct MsgNpcSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public ushort X;
        public ushort Y;
        public ushort Look;
        public ushort Type;
        public ushort Sort;
        public ushort Base;

        public static byte[] Create(PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<BodyComponent>();
            ref readonly var npc = ref ntt.Get<NpcComponent>();
            
            var packet = new MsgNpcSpawn
            {
                Size = (ushort)sizeof(MsgNpcSpawn),
                Id = 2030,
                UniqueId = ntt.Id,
                Look = (ushort)phy.Look,
                X = (ushort)phy.Location.X,
                Y = (ushort)phy.Location.Y,
                Type = npc.Type,
                Sort = npc.Sort,
                Base = npc.Base
            };
            return packet;
        }

        public static implicit operator byte[](MsgNpcSpawn msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgNpcSpawn*)p = *&msg;
            return buffer;
        }
    }
}