using System.Buffers;
using System.Runtime.InteropServices;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgMapStatus
    {
        public ushort Size;
        public ushort Id;
        public uint MapId;
        public uint MinimapId;
        public uint Flags;

        public static MsgMapStatus Create(uint mapId, uint flags)
        {
            var msg = new MsgMapStatus
            {
                Size = (ushort)sizeof(MsgMapStatus),
                Id = 1110,
                MapId = mapId,
                MinimapId = mapId,
                Flags = flags,
            };
            return msg;
        }
    }
}