using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 36)]
    public unsafe struct MsgDynNpcSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public uint MaximumHp;
        public uint CurrentHp;
        public short X;
        public short Y;
        public ushort Look;
        public ushort Type;
        public ushort Base;

        public static byte[] Create(PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<BodyComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var npc = ref ntt.Get<NpcComponent>();
            var packet = new MsgDynNpcSpawn
            {
                Size = (ushort)sizeof(MsgDynNpcSpawn),
                Id = 1109,
                UniqueId = ntt.Id,
                CurrentHp = hlt.Health,
                MaximumHp = hlt.MaxHealth,
                X = (short)phy.Location.X,
                Y = (short)phy.Location.Y,
                Look = (ushort)phy.Look,
                Type = npc.Type,
                Base = npc.Base
            };
            return packet;
        }

        public static implicit operator byte[](MsgDynNpcSpawn msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgDynNpcSpawn*)p = *&msg;
            return buffer;
        }
    }
}