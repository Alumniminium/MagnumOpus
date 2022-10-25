using System.Runtime.InteropServices;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgCompose
    {
        public ushort Size;
        public ushort Id;
        public int UnqiueId;
        public int FirstItemUID;
        public int SecondItemUID;
        public int FirstGemUID;
        public int SecondGemUID;
    }
}