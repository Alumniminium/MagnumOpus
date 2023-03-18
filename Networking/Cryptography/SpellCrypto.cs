using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking.Cryptography
{
    public static class SpellCrypto
    {
        public static (ushort id, int target, ushort x, ushort y) DecryptSkill(in NTT player, ref MsgInteract packet)
        {
            ref var net = ref player.Get<NetworkComponent>();
            var buffer = Co2Packet.Serialize(ref packet).Span;
            var id = Convert.ToUInt16(((long)buffer[24] & 0xFF) | (((long)buffer[25] & 0xFF) << 8));

            id ^= 0x915d;
            id ^= (ushort)player.Id;
            id = (ushort)((id << 0x3) | (id >> 0xd));
            id -= 0xeb42;

            long x = (buffer[16] & 0xFF) | ((buffer[17] & 0xFF) << 8);
            long y = (buffer[18] & 0xFF) | ((buffer[19] & 0xFF) << 8);

            x = x ^ (player.Id & 0xffff) ^ 0x2ed6;
            x = ((x << 1) | ((x & 0x8000) >> 15)) & 0xffff;
            x |= 0xffff0000;
            x -= 0xffff22ee;

            y = y ^ (player.Id & 0xffff) ^ 0xb99b;
            y = ((y << 5) | ((y & 0xF800) >> 11)) & 0xffff;
            y |= 0xffff0000;
            y -= 0xffff8922;

            var target = BitConverter.ToUInt32(buffer[12..]);
            target = (uint)((((target & 0xffffe000) >> 13) | ((target & 0x1fff) << 19)) ^ 0x5F2D2463 ^ player.Id) - 0x746F4AE6;
            return (id, (int)target, (ushort)x, (ushort)y);
        }
    }
}