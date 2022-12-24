using System.Runtime.InteropServices;
using System.Text;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgAccount
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public fixed byte Username[16];
        [FieldOffset(20)]
        public fixed byte Password[16];
        [FieldOffset(36)]
        public fixed byte Server[16];
        
        public string GetUsername()
        {
            fixed (byte* ptr = Username)
                return Encoding.ASCII.GetString(ptr, 16).Trim('\0');
        }        

        public string GetPassword()
        {
            fixed (byte* ptr = Password)
                return Encoding.ASCII.GetString(ptr, 16).Trim('\0');
        }

        public string GetServer()
        {
            fixed (byte* ptr = Server)
                return Encoding.ASCII.GetString(ptr, 16).Trim('\0');
        }
    }
}