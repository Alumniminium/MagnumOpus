using System.Buffers;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgWalk
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public Direction Direction;
        public byte Type;

        public static Memory<byte> Create(int uniqueId, Direction direction, bool running)
        {
            MsgWalk* msgP = stackalloc MsgWalk[1];

            msgP->Size = (ushort)sizeof(MsgWalk);
            msgP->Id = 1005;
            msgP->UniqueId = uniqueId;
            msgP->Direction = direction;
            msgP->Type = running ? (byte)1 : (byte)0;

            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *msgP;
            return buffer;
        }

        [PacketHandler(PacketId.MsgWalk)]
        public static void Process(PixelEntity ntt, Memory<byte> packet)
        {
            var msg = (MsgWalk)packet;

            if (ntt.Id != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgWalk)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.Id}");

            var wlk = new WalkComponent(ntt.Id, msg.Direction, msg.Type == 1);
            ntt.Add(ref wlk);
        }

        public static implicit operator Memory<byte>(MsgWalk msg)
        {
            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *&msg;
            return buffer;
        }
        public static implicit operator MsgWalk(Memory<byte> buffer)
        {
            MsgWalk msg;
            fixed (byte* p = buffer.Span)
                msg = *(MsgWalk*)p;

            msg.Direction = (Direction)((byte)msg.Direction% 8);
            return msg;
        }
    }
}