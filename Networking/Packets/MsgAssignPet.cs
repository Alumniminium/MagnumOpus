using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Components.Location;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgAssignPet
    {
        public ushort Size;
        public ushort Id;
        public int UnqiueId;
        public uint Model;
        public uint AI;
        public ushort X;
        public ushort Y;
        public fixed byte Summoner[16];

        public static MsgAssignPet Create(in NTT obj, int uid)
        {
            ref readonly var bdy = ref obj.Get<BodyComponent>();
            ref readonly var pos = ref obj.Get<PositionComponent>();
            var packet = new MsgAssignPet
            {
                Size = 36,
                Id = 2035,
                Model = 920,
                AI = 1,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                UnqiueId = uid,
            };
            for (byte i = 0; i < "GoldGuard".Length; i++)
                packet.Summoner[i] = (byte)"GoldGuard"[i];

            return packet;
        }
    }
}