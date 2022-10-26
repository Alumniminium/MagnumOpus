using System.Runtime.InteropServices;
using System.Text;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgDHX
    {
        [FieldOffset(0)]
        public fixed byte Padding[11];
        [FieldOffset(11)]
        public uint Size;
        [FieldOffset(15)]
        public uint RandomPadLen;
        [FieldOffset(19)]
        public fixed byte RandomPad[42];
        [FieldOffset(61)]
        public uint EncryptionIVLen;
        [FieldOffset(65)]
        public fixed byte EncryptionIV[8];
        [FieldOffset(73)]
        public uint DecryptionIVLen;
        [FieldOffset(77)]
        public fixed byte DecryptionIV[8];
        [FieldOffset(85)]
        public uint PrimeModuloLen;
        [FieldOffset(89)]
        public fixed byte PrimeModulo[128];
        [FieldOffset(217)]
        public uint GeneratorLen;
        [FieldOffset(221)]
        public fixed byte Generator[2];
        [FieldOffset(223)]
        public uint PublicKeyLen;
        [FieldOffset(227)]  
        public fixed byte PublicKey[128];

        public static MsgDHX Create(in Memory<byte> civ, Memory<byte> siv, string p, string g, string pubkey)
        {
            var dhx = new MsgDHX
            {
                Size = 352,
                RandomPadLen = 42,
                EncryptionIVLen = 8,
                DecryptionIVLen = 8,
                PrimeModuloLen = (uint)p.Length,
                GeneratorLen = (uint)g.Length,
                PublicKeyLen = (uint)pubkey.Length
            };

            for (var i = 0; i < 8; i++)
                dhx.EncryptionIV[i] = civ.Span[i];
            for (var i = 0; i < 8; i++)
                dhx.DecryptionIV[i] = siv.Span[i];
            for (var i = 0; i < p.Length; i++)
                dhx.PrimeModulo[i] = (byte)p[i];
            for (var i = 0; i < g.Length; i++)
                dhx.Generator[i] = (byte)g[i];
            for (var i = 0; i < pubkey.Length; i++)
                dhx.PublicKey[i] = (byte)pubkey[i];

            return dhx;
        }

        public static implicit operator Memory<byte>(MsgDHX msg) 
        {
            var buffer = new byte[sizeof(MsgDHX)];
            fixed (byte* ptr = buffer)
                *(MsgDHX*)ptr = msg;
            return buffer;
        }
    }
}