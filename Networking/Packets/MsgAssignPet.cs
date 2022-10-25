using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgAssignPet
    {
        public ushort Size;
        public ushort Id;
        public int UnqiueId;
        public uint Model;
        public uint AI;
        public ushort X;
        public ushort Y;
        public fixed byte Summoner[16];

        public static byte[] Create(PixelEntity obj, int uid)
        {
            ref readonly var phy = ref obj.Get<BodyComponent>();
            var packet = new MsgAssignPet
            {
                Size = 36,
                Id = 2035,
                Model = 920,
                AI = 1,
                X = (ushort)phy.Location.X,
                Y = (ushort)phy.Location.Y,
                UnqiueId = uid,
            };
            for (byte i = 0; i < "GoldGuard".Length; i++)
                packet.Summoner[i] = (byte)"GoldGuard"[i];

            return packet;
        }

        public static implicit operator byte[](MsgAssignPet msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgAssignPet*)p = *&msg;
            return buffer;
        }
    }
}