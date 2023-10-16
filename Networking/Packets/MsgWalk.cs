using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

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
                IDK = (ushort)NttWorld.Tick,
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
                IDK = (ushort)NttWorld.Tick,
            };
            return msg;
        }

        [PacketHandler(PacketId.MsgWalk)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialize<MsgWalk>(memory.Span);
            if (ntt.Id != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgWalk)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.Id}");

            ref var emo = ref ntt.Get<EmoteComponent>();
            if (emo.Emote != Emote.Stand)
            {
                emo.Emote = Emote.Stand;
                emo.ChangedTick = NttWorld.Tick;
            }

            var wlk = new WalkComponent(msg.RawDirection, msg.Type == 1);
            ntt.Set(ref wlk);

            ntt.Remove<AttackComponent>();
        }
    }
}