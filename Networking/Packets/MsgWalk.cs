using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
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

        public static MsgWalk Create(int uniqueId, byte direction, bool running)
        {
            var msg = new MsgWalk
            {
                Size = (ushort)sizeof(MsgWalk),
                Id = 1005,
                UniqueId = uniqueId,
                RawDirection = direction,
                Type = running ? (byte)1 : (byte)0,
                IDK = (ushort)PixelWorld.Tick,
            }; 
            return msg;
        }
        public static MsgWalk Create(int uniqueId, Direction direction, bool running)
        {
            var msg = new MsgWalk
            {
                Size = (ushort)sizeof(MsgWalk),
                Id = 1005,
                UniqueId = uniqueId,
                Direction = direction,
                Type = running ? (byte)1 : (byte)0,
                IDK = (ushort)PixelWorld.Tick,
            }; 
            return msg;
        }

        [PacketHandler(PacketId.MsgWalk)]
        public static void Process(PixelEntity ntt, Memory<byte> packet)
        {
            var msg = Co2Packet.Deserialze<MsgWalk>(packet);
            if (ntt.NetId != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgWalk)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.NetId}");

            if (ntt.Has<WalkComponent>())
            {
                ref var pos = ref ntt.Get<PositionComponent>();
                pos.ChangedTick = PixelWorld.Tick;
                var kickback = MsgAction.Create(ntt.NetId, 0, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.South, MsgActionType.Kickback);
                ntt.NetSync(ref kickback);
            }
            var wlk = new WalkComponent(ntt.Id, msg.RawDirection, msg.Type == 1);
            ntt.Add(ref wlk);
        }
    }
}