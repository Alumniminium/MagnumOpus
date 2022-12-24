using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTick
    {
        public ushort Size;
        public PacketId Id;
        public int UniqueId;
        public int Timestamp;
        public uint Junk1;
        public uint Junk2;
        public uint Junk3;
        public uint Junk4;
        public uint Hash;

        public static MsgTick Create(in PixelEntity target)
        {
            var msg = new MsgTick
            {
                Size = (ushort)sizeof(MsgTick),
                Id = PacketId.MsgTick,
                UniqueId = target.NetId,
                Timestamp = Environment.TickCount,
                Junk1 = 0,
                Junk2 = 0,
                Junk3 = 0,
                Junk4 = 0,
                Hash = 0,
            };
            return msg;
        }

        [PacketHandler(PacketId.MsgTick)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgTick>(memory);
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            if (!ntt.Has<PingComponent>())
            {
                var ping = new PingComponent(ntt.Id);
                ntt.Add(ref ping);
            }
            ref var pin = ref ntt.Get<PingComponent>();

            if (ntt.NetId != msg.UniqueId)
                FConsole.WriteLine($"UID Mismatch! {msg.UniqueId}");
            if (msg.Hash != HashName(ntc.Name))
                FConsole.WriteLine($"Hash Mismatch! {msg.Hash}");

            msg.Timestamp ^= msg.UniqueId;
            pin.Ping = Math.Abs(msg.Timestamp - pin.LastPing - 10000);
            pin.LastPing = msg.Timestamp;
            
            var pingMsg = MsgText.Create("SYSTEM", "PING", $"10s Avg Ping: {pin.Ping}ms", MsgTextType.MiniMap);
            ntt.NetSync(in pingMsg);
        }

        public static uint HashName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 4)
                return 0x9D4B5703;

            var buffer = Encoding.GetEncoding("iso-8859-1").GetBytes(name);
            fixed (byte* pBuf = buffer)
                return ((ushort*)pBuf)[0] ^ 0x9823U;
        }
    }
}