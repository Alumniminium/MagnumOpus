using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFriendInfo
    {
        public ushort Size;
        public ushort Id;
        public int UniqId;
        public uint Look;
        public byte Level;
        public byte Profession;
        public ushort PkPoints;
        public ushort GuildUID;
        public byte Unknow;
        public byte Position;
        public fixed byte Spouse[16];

        public static MsgFriendInfo Create(in PixelEntity target)
        {
            ref readonly var trs = ref target.Get<TransformationComponent>();
            ref readonly var bdy = ref target.Get<BodyComponent>();
            ref readonly var gld = ref target.Get<GuildComponent>();
            ref readonly var mar = ref target.Get<MarriageComponent>();
            ref readonly var pkp = ref target.Get<PkPointComponent>();
            ref readonly var lvl = ref target.Get<LevelComponent>();
            ref readonly var pro = ref target.Get<ProfessionComponent>();

            ref readonly var spouse = ref PixelWorld.GetEntity(mar.SpouseId);
            ref readonly var spouseNtc = ref spouse.Get<NameTagComponent>();
            
            var packet = new MsgFriendInfo
            {
                Size = (ushort)sizeof(MsgFriendInfo),
                Id = 2033,
                UniqId = target.NetId,
                Look = bdy.Look,
                Level = lvl.Level,
                Profession = (byte)pro.Class,
                PkPoints = pkp.Points,
                GuildUID = 0,
                Unknow = 0,
                Position = (byte)gld.Rank,
            };
            
            for (byte i = 0; i < spouseNtc.Name.Length; i++)
                packet.Spouse[i] = (byte)spouseNtc.Name[i];
            return packet;
        }
    }
}