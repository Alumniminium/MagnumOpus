using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking.Cryptography
{
    public static class SpellCrypto
    {
        public static (ushort id, int target, ushort x, ushort y) DecryptSkill(in NTT player, ref MsgInteract packet)
        {
<<<<<<< HEAD
            ref var net = ref player.Get<NetworkComponent>();
            Co2Packet.Serialize(ref net, ref packet);
            var buffer = net.SendBuffer[(net.SendBufferOffset - packet.Size)..].Span;
            var id = Convert.ToUInt16(((long)buffer[24] & 0xFF) | (((long)buffer[25] & 0xFF) << 8));
=======
            var buffer = Co2Packet.Serialize(ref packet);
            var id = Convert.ToUInt16(((long)buffer.Span[24] & 0xFF) | (((long)buffer.Span[25] & 0xFF) << 8));
>>>>>>> 3161883ebe68e1efedc7baa80d392375ebd53323

            id ^= 0x915d;
            id ^= (ushort)player.Id;
            id = (ushort)((id << 0x3) | (id >> 0xd));
            id -= 0xeb42;

<<<<<<< HEAD
            long x = (buffer[16] & 0xFF) | ((buffer[17] & 0xFF) << 8);
            long y = (buffer[18] & 0xFF) | ((buffer[19] & 0xFF) << 8);
=======
            long x = (buffer.Span[16] & 0xFF) | ((buffer.Span[17] & 0xFF) << 8);
            long y = (buffer.Span[18] & 0xFF) | ((buffer.Span[19] & 0xFF) << 8);
>>>>>>> 3161883ebe68e1efedc7baa80d392375ebd53323

            x = x ^ (player.Id & 0xffff) ^ 0x2ed6;
            x = ((x << 1) | ((x & 0x8000) >> 15)) & 0xffff;
            x |= 0xffff0000;
            x -= 0xffff22ee;

            y = y ^ (player.Id & 0xffff) ^ 0xb99b;
            y = ((y << 5) | ((y & 0xF800) >> 11)) & 0xffff;
            y |= 0xffff0000;
            y -= 0xffff8922;

<<<<<<< HEAD
            var target = BitConverter.ToUInt32(buffer[12..]);
=======
            var target = BitConverter.ToUInt32(buffer.Span[12..]);
>>>>>>> 3161883ebe68e1efedc7baa80d392375ebd53323
            target = (uint)((((target & 0xffffe000) >> 13) | ((target & 0x1fff) << 19)) ^ 0x5F2D2463 ^ player.Id) - 0x746F4AE6;
            return (id, (int)target, (ushort)x, (ushort)y);
        }
    }
}