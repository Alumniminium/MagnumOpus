using System.Runtime.InteropServices;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgText
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int Color;
        [FieldOffset(8)]
        public MsgTextType Channel;
        [FieldOffset(10)]
        public ushort Style;
        [FieldOffset(12)]
        public int SenderUniqueId;
        [FieldOffset(16)]
        public int Look1;
        [FieldOffset(20)]
        public int Look2;
        [FieldOffset(24)]
        public byte StringCount;
        [FieldOffset(25)]
        public fixed byte Data[304];

        public unsafe string From()
        {
            fixed (byte* p = Data)
            {
                var fromLen = p[0];
                var txtBytes = new byte[fromLen];
                for (var i = 0; i < txtBytes.Length; i++)
                    txtBytes[i] = p[1 + i];
                return Encoding.ASCII.GetString(txtBytes);
            }
        }

        public unsafe string To()
        {
            fixed (byte* p = Data)
            {
                var fromLen = p[0];
                var toLen = p[1 + fromLen];
                var txtBytes = new byte[toLen];
                for (var i = 0; i < txtBytes.Length; i++)
                    txtBytes[i] = p[2 + fromLen + i];
                return Encoding.ASCII.GetString(txtBytes);
            }
        }

        public unsafe string Message()
        {
            fixed (byte* p = Data)
            {
                var fromLen = p[0];
                var toLen = p[1 + fromLen];
                var emoLen = p[2 + fromLen + toLen];
                var msgLen = p[3 + emoLen + fromLen + toLen];
                var txtBytes = new byte[msgLen];
                for (var i = 0; i < txtBytes.Length; i++)
                    txtBytes[i] = p[4 + fromLen + toLen + emoLen + i];
                return Encoding.ASCII.GetString(txtBytes);
            }
        }
        public static MsgText Create(in PixelEntity to, string message, MsgTextType type = MsgTextType.Talk)
        {
            ref readonly var ntc = ref to.Get<NameTagComponent>();
            return Create(ntc.Name, ntc.Name, message, type);
        }
        public static MsgText Create(string from, string to, string message, MsgTextType type)
        {
            from = from.Replace("\0", "");
            to = to.Replace("\0", "");
            message = message.Replace("\0", "");

            var packet = new MsgText
            {
                Size = (ushort)(29 + from.Length + to.Length + message.Length),
                Id = 1004,
                StringCount = 4,
                Channel = type,
                SenderUniqueId = Environment.TickCount,
                // Color = 0x00FF00FF,
            };

            // if (GameWorld.Find(to, out YiObj found))
            //     packet.Look1 = (int)found.Look;
            // if (GameWorld.Find(from, out YiObj found1))
            //     packet.Look2 = (int)found1.Look;


            packet.Data[0] = (byte)from.Length;
            for (var i = 0; i < from.Length; i++)
                packet.Data[1 + i] = (byte)from[i];
            packet.Data[1 + from.Length] = (byte)to.Length;
            for (var i = 0; i < to.Length; i++)
                packet.Data[2 + from.Length + i] = (byte)to[i];
            packet.Data[2 + from.Length + to.Length] = 0;
            packet.Data[3 + from.Length + to.Length] = (byte)message.Length;
            for (var i = 0; i < message.Length; i++)
                packet.Data[4 + from.Length + to.Length + i] = (byte)message[i];
            return packet;
        }

        [PacketHandler(PacketId.MsgText)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgText>(memory);
            FConsole.WriteLine($"MsgText: {msg.Channel} {msg.From()} -> {msg.To()}: {msg.Message()}");

            switch (msg.Channel)
            {
                case MsgTextType.Ghost:
                    GhostChat(in ntt, ref msg);
                    break;
                case MsgTextType.Talk:
                    TalkChat(in ntt, ref msg);
                    break;
                default:
                    FConsole.WriteLine("Unknown ChatType: " + msg.Channel);
                    FConsole.WriteLine(memory.Dump());
                    break;
            }
        }

        private static void GhostChat(in PixelEntity ntt, ref MsgText mem)
        {
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            foreach (var entity in vwp.EntitiesVisible)
            {
                ref readonly var job = ref entity.Get<ProfessionComponent>();

                switch (job.Class)
                {
                    case ClasseName.WaterMaster:
                    case ClasseName.WaterSaint:
                    case ClasseName.WaterTaoist:
                    case ClasseName.WaterWizard:
                        {
                            entity.NetSync(ref mem);
                            break;
                        }
                }
            }
        }

        private static void TalkChat(in PixelEntity ntt, ref MsgText mem)
        {
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            foreach (var entity in vwp.EntitiesVisible)
                if (entity != ntt)
                    entity.NetSync(ref mem);

            var txt = mem.Message();
            var command = txt.Replace("/", "").Split(' ')[0];
            var args = txt.Replace("/", "").Split(' ').ToArray()[1..];

            switch (command)
            {
                case "cc":
                    ref var pos = ref ntt.Get<PositionComponent>();
                    pos.Position.X = ushort.Parse(args[0]);
                    pos.Position.Y = ushort.Parse(args[1]);
                    pos.Map = ushort.Parse(args[2]);
                    var tpc = new TeleportComponent(ntt.Id, (ushort)pos.Position.X, (ushort)pos.Position.Y, (ushort)pos.Map);
                    ntt.Set(ref tpc);
                    break;
                case "rev":
                    var rev = new ReviveComponent(ntt.Id, 1);
                    ntt.Set(ref rev);
                    break;
            }
        }
    }
}