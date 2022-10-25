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

        public static byte[] Create(PixelEntity source, PixelEntity target, MsgInteractType type, int value)
        {
            ref readonly var phy = ref target.Get<BodyComponent>();
                        
            var msgP = stackalloc MsgInteract[1];
            msgP->Size = (ushort)sizeof(MsgInteract);
            msgP->Id = 1022;
            msgP->Timestamp = Environment.TickCount;
            msgP->AttackerUniqueId = source.Id;
            msgP->TargetUniqueId = target.Id;
            msgP->X = (ushort)phy.Location.X;
            msgP->Y = (ushort)phy.Location.Y;
            msgP->Type = type;
            msgP->Value = value;

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }
        public static byte[] Create(int attackerUniqueId, int targetUniqueId, ushort targetX, ushort targetY, MsgInteractType type, int value)
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

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }
        public static byte[] Die(PixelEntity attacker, PixelEntity target)
        {
            ref readonly var phy = ref target.Get<BodyComponent>();
            var msgP = stackalloc MsgInteract[1];
            msgP->Size = 32;
            msgP->Id = 1022;
            msgP->Timestamp = Environment.TickCount;
            msgP->AttackerUniqueId = attacker.Id;
            msgP->TargetUniqueId = target.Id;
            msgP->X = (ushort)phy.Location.X;
            msgP->Y = (ushort)phy.Location.Y;
            msgP->Type = MsgInteractType.Death;
            msgP->Value = 0;

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *msgP;
            return buffer;
        }

        public static implicit operator byte[](MsgInteract msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgInteract*)p = *&msg;
            return buffer;
        }
    }
}