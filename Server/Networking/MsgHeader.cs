using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MsgHeader
{
    public ushort Size;
    public PacketId Id;
}
