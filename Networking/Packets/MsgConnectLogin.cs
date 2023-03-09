using System.Runtime.InteropServices;
using System.Text;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgConnectLogin
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public uint UniqueId;
        [FieldOffset(8)]
        public uint Contents;

        [FieldOffset(12)]
        public fixed byte FileName[16];

        public string GetFileName()
        {
            fixed (byte* ptr = FileName)
                return Encoding.ASCII.GetString(ptr, 16).Trim('\0');
        }
    }
}