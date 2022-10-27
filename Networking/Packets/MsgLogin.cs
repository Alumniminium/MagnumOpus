using System.Runtime.InteropServices;
using System.Text;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgLogin
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public ulong Token;
        [FieldOffset(4)]
        public uint UniqueId;
        [FieldOffset(12)]
        public ushort ClientVersion;
        [FieldOffset(14)]
        public fixed byte Language[10];
        [FieldOffset(24)]
        public uint Contents;

        public string GetLanguage()
        {
            fixed (byte* ptr = Language)
                return Encoding.ASCII.GetString(ptr, 10).Trim('\0');
        }

        [PacketHandler(PacketId.MsgLogin)]
        public static void Process(PixelEntity ntt, Memory<byte> packet)
        {
            var msg = (MsgLogin)packet;
            var language = msg.GetLanguage();
            FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");
            var ntc = new NameTagComponent(ntt.Id, "test");
            var dir = new DirectionComponent(ntt.Id, Direction.South);
            ntt.Add(ref ntc);
            ntt.Add(ref dir);

            var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
            ntt.NetSync(in ok);

            var info = MsgCharacter.Create(ntt);
            ntt.NetSync(in info);
        }

        public static implicit operator MsgLogin(in Memory<byte> msg)
        {
            fixed (byte* ptr = msg.Span)
                return *(MsgLogin*)ptr;
        }
    }
}