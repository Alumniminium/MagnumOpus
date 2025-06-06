using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 36)]
    public unsafe struct MsgDynNpcSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public uint MaximumHp;
        public uint CurrentHp;
        public short X;
        public short Y;
        public ushort Look;
        public ushort Type;
        public ushort Base;

        public static MsgDynNpcSpawn Create(in NTT ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var npc = ref ntt.Get<NpcComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var packet = new MsgDynNpcSpawn
            {
                Size = (ushort)sizeof(MsgDynNpcSpawn),
                Id = 1109,
                UniqueId = ntt.Id,
                CurrentHp = (ushort)hlt.Health,
                MaximumHp = (ushort)hlt.MaxHealth,
                X = (short)pos.Position.X,
                Y = (short)pos.Position.Y,
                Look = (ushort)bdy.Look,
                Type = npc.Type,
                Base = npc.Base
            };
            return packet;
        }
    }
}