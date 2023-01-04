using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgMagicEffect
    {
        public ushort Size;
        public ushort Id;
        public int UniqId;
        // public int Param; //TargetUID || (X, Y)
        public ushort X;
        public ushort Y;
        public ushort Type;
        public short Level;
        public int TargetCount;
        public fixed int Targets[180];

        // public static IEnumerable<byte[]> Create(in PixelEntity attacker, IEnumerable<(in PixelEntity, int)> targetEnumerable)
        // {
        //     ref readonly var bdy = ref attacker.Get<PhysicsComponent>();
            
        //     const int maxTargets = 60;
        //     var targets = targetEnumerable.ToDictionary(pair => pair.Item1, pair => pair.Item2);
        //     var packetCount = (int)Math.Max(1, Math.Ceiling((float)targets.Count / maxTargets));
        //     var packets = new byte[packetCount][];
        //     var entities = targets.Keys.ToArray();
        //     var damages = targets.Values.ToArray();
        //     for (var i = 0; i < packetCount; i++)
        //     {
        //         var packet = stackalloc MsgMagicEffect[1];
        //         {
        //             packet->Size = (ushort)(28 + 12 * Math.Min(Math.Min(targets.Count - i * maxTargets, maxTargets), targets.Count));
        //             packet->Id = 1105;
        //             packet->UniqId = attacker.Id;
        //             packet->Param = (int)bdy.Direction;
        //             packet->Type = attacker.CurrentSkill.Id;
        //             packet->Level = attacker.CurrentSkill.Level;
        //             packet->TargetCount = Math.Min(Math.Min(targets.Count - i * maxTargets, maxTargets), targets.Count);
        //         };
        //         var offset = 0;
        //         for (var j = 0; j < Math.Min(targets.Count - i * maxTargets, maxTargets); j++)
        //         {
        //             packet->Targets[offset++] = entities[j + i * maxTargets].Id;
        //             packet->Targets[offset++] = damages[j + i * maxTargets];
        //             packet->Targets[offset++] = 0;
        //         }

        //         var buffer = new byte[sizeof(MsgUpdate));
        //         fixed (byte* p = buffer)
        //             *(MsgMagicEffect*)p = *packet;
        //         packets[i] = buffer;
        //     }
        //     return packets;
        // }

        public static Memory<byte> Create(in PixelEntity attacker, in PixelEntity target, int damage, ushort skillId, byte skillLevel)
        {
            var buffer = new byte[sizeof(MsgMagicEffect)];
            var packet = new MsgMagicEffect()
            {
                Size = 28,
                Id = 1105,
                UniqId = attacker.NetId,
                // Param = (int)attacker.Get<DirectionComponent>().Direction,
                X = (ushort)target.Get<PositionComponent>().Position.X,
                Y = (ushort)target.Get<PositionComponent>().Position.Y,
                Type = skillId,
                Level = skillLevel,
                TargetCount = 1
            };
            unsafe
            {
                packet.Targets[0] = target.NetId;
                packet.Targets[1] = damage;
                packet.Targets[2] = 0;
                fixed (byte* p = buffer)
                    *(MsgMagicEffect*)p = packet;
            }


            return buffer.AsMemory()[..packet.Size];
        }
    }
}