using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

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

        public static Memory<byte> Create(in PixelEntity source, PixelEntity target, MsgInteractType type, int value)
        {
            ref readonly var bdy = ref target.Get<BodyComponent>();
            ref readonly var pos = ref target.Get<PositionComponent>();
                        
            var msgP = stackalloc MsgInteract[1];
            msgP->Size = (ushort)sizeof(MsgInteract);
            msgP->Id = 1022;
            msgP->Timestamp = Environment.TickCount;
            msgP->AttackerUniqueId = source.Id;
            msgP->TargetUniqueId = target.Id;
            msgP->X = (ushort)pos.Position.X;
            msgP->Y = (ushort)pos.Position.Y;
            msgP->Type = type;
            msgP->Value = value;

            var buffer = new byte[sizeof(MsgInteract)];
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }
        public static Memory<byte> Create(int attackerUniqueId, int targetUniqueId, ushort targetX, ushort targetY, MsgInteractType type, int value)
        {
            var msgP = stackalloc MsgInteract[1];
            msgP->Size = (ushort)sizeof(MsgInteract);
            msgP->Id = 1022;
            msgP->Timestamp = Environment.TickCount;
            msgP->AttackerUniqueId = attackerUniqueId;
            msgP->TargetUniqueId = targetUniqueId;
            msgP->X = targetX;
            msgP->Y = targetY;
            msgP->Type = type;
            msgP->Value = value;

            var buffer = new byte[sizeof(MsgInteract)];
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }
        public static Memory<byte> Die(in PixelEntity attacker, PixelEntity target)
        {
            ref readonly var bdy = ref target.Get<PositionComponent>();
            var msgP = stackalloc MsgInteract[1];
            msgP->Size = 32;
            msgP->Id = 1022;
            msgP->Timestamp = Environment.TickCount;
            msgP->AttackerUniqueId = attacker.Id;
            msgP->TargetUniqueId = target.Id;
            msgP->X = (ushort)bdy.Position.X;
            msgP->Y = (ushort)bdy.Position.Y;
            msgP->Type = MsgInteractType.Death;
            msgP->Value = 0;

            var buffer = new byte[sizeof(MsgInteract)];
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }

        public static implicit operator Memory<byte>(MsgInteract msg)
        {
            var buffer = new byte[sizeof(MsgInteract)];
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *&msg;
            return buffer;
        }
    }
}