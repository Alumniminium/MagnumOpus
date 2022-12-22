using System.Buffers;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;
using System.Diagnostics;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgWalk
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int UniqueId;
        [FieldOffset(8)]
        public byte RawDirection;
        [FieldOffset(8)]
        public Direction Direction;
        [FieldOffset(9)]
        public byte Type;
        [FieldOffset(10)]
        public ushort IDK;
        
        public static Memory<byte> Create(int uniqueId, byte direction, bool running)
        {
            MsgWalk* msgP = stackalloc MsgWalk[1];

            msgP->Size = (ushort)sizeof(MsgWalk);
            msgP->Id = 1005;
            msgP->UniqueId = uniqueId;
            msgP->RawDirection = direction;
            msgP->Type = running ? (byte)1 : (byte)0;
            msgP->IDK = (ushort)PixelWorld.Tick;

            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *msgP;
            return buffer;
        }
        public static Memory<byte> Create(int uniqueId, Direction direction, bool running)
        {
            MsgWalk* msgP = stackalloc MsgWalk[1];

            msgP->Size = (ushort)sizeof(MsgWalk);
            msgP->Id = 1005;
            msgP->UniqueId = uniqueId;
            msgP->Direction =direction;
            msgP->Type = running ? (byte)1 : (byte)0;
            msgP->IDK = (ushort)PixelWorld.Tick;

            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *msgP;
            return buffer;
        }

        [PacketHandler(PacketId.MsgWalk)]
        public static void Process(PixelEntity ntt, Memory<byte> packet)
        {
            var msg = (MsgWalk)packet;
            FConsole.WriteLine($"IDK:{msg.IDK}");
            if (ntt.NetId != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgWalk)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.NetId}");

            if(ntt.Has<WalkComponent>())
            {
                ref var pos = ref ntt.Get<PositionComponent>();
                pos.ChangedTick = PixelWorld.Tick;
                ntt.NetSync(MsgAction.Create((int)PixelWorld.Tick, ntt.NetId, 0, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.South, MsgActionType.Kickback));
                return;
            }
            var wlk = new WalkComponent(ntt.Id, msg.RawDirection, msg.Type == 1);
            ntt.Add(ref wlk);
        }

        internal static Memory<byte> Create(int netId, object rawDirection, bool isRunning)
        {
            throw new NotImplementedException();
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
            fixed (byte* p = buffer.Span)
                return *(MsgWalk*)p;
        }
    }
}