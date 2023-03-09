using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Components.Location;
using MagnumOpus.Components.CQ;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 101)]
    public unsafe struct MsgNpcSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public ushort X;
        public ushort Y;
        public ushort Look;
        public ushort Type;
        public ushort Sort;
        public ushort Base;

        public static MsgNpcSpawn Create(in NTT ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var npc = ref ntt.Get<NpcComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var packet = new MsgNpcSpawn
            {
                Size = (ushort)sizeof(MsgNpcSpawn),
                Id = 2030,
                UniqueId = ntt.Id,
                Look = (ushort)bdy.Look,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Type = npc.Type,
                Sort = npc.Sort,
                Base = npc.Base
            };
            return packet;
        }
    }
}