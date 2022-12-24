using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgInteract
    {
        public ushort Size;
        public ushort Id;
        public int Timestamp;
        public int AttackerUniqueId;
        public int TargetUniqueId;
        public ushort X;
        public ushort Y;
        public MsgInteractType Type;
        public int Value;

        public static MsgInteract Create(in PixelEntity source, PixelEntity target, MsgInteractType type, int value)
        {
            ref readonly var bdy = ref target.Get<BodyComponent>();
            ref readonly var pos = ref target.Get<PositionComponent>();

            var msg = new MsgInteract
            {
                Size = (ushort)sizeof(MsgInteract),
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = source.NetId,
                TargetUniqueId = target.NetId,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Type = type,
                Value = value,
            };
            return msg;
        }
        public static MsgInteract Create(int attackerUniqueId, int targetUniqueId, ushort targetX, ushort targetY, MsgInteractType type, int value)
        {
            var msg = new MsgInteract
            {
                Size = (ushort)sizeof(MsgInteract),
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = attackerUniqueId,
                TargetUniqueId = targetUniqueId,
                X = targetX,
                Y = targetY,
                Type = type,
                Value = value,
            };
            return msg;
        }
        public static MsgInteract Die(in PixelEntity attacker, PixelEntity target)
        {
            ref readonly var bdy = ref target.Get<PositionComponent>();
            var msg = new MsgInteract
            {
                Size = 32,
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = attacker.NetId,
                TargetUniqueId = target.NetId,
                X = (ushort)bdy.Position.X,
                Y = (ushort)bdy.Position.Y,
                Type = MsgInteractType.Death,
                Value = 0,
            };

            return msg;
        }
    }
}