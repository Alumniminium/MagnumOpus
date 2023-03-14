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
            var msg = Co2Packet.Deserialze<MsgWalk>(memory);
            if (ntt.Id != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgWalk)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.Id}");

            if (ntt.Has<WalkComponent>())
            {
                ref var pos = ref ntt.Get<PositionComponent>();
                pos.ChangedTick = NttWorld.Tick;
                var kickback = MsgAction.Create(ntt.Id, 0, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.South, MsgActionType.Kickback);
                ntt.NetSync(ref kickback, true);
                return;
            }

            var wlk = new WalkComponent(msg.RawDirection, msg.Type == 1);
            var emo = new EmoteComponent(Emote.Stand);
            ntt.Set(ref wlk);
            ntt.Set(ref emo);
            ntt.Remove<AttackComponent>();
        }
    }
}