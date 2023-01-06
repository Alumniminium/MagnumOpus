using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgCharacter
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int EntityId;
        [FieldOffset(8)]
        public uint Look;
        [FieldOffset(12)]
        public ushort Hair;
        [FieldOffset(14)]
        public uint Money;
        [FieldOffset(18)]
        public uint CPs;
        [FieldOffset(22)]
        public ulong Experience;
        [FieldOffset(46)]
        public ushort Strength;
        [FieldOffset(48)]
        public ushort Agility;
        [FieldOffset(50)]
        public ushort Vitality;
        [FieldOffset(52)]
        public ushort Spirit;
        [FieldOffset(54)]
        public ushort Statpoints;
        [FieldOffset(56)]
        public ushort Health;
        [FieldOffset(58)]
        public ushort Mana;
        [FieldOffset(60)]
        public ushort PkPoints;
        [FieldOffset(62)]
        public byte Level;
        [FieldOffset(63)]
        public byte Class;
        [FieldOffset(64)]
        public byte PrevClass;
        [FieldOffset(65)]
        public byte Reborn;
        [FieldOffset(66)]
        public bool ShowName;
        [FieldOffset(67)]
        public byte StringCount;
        [FieldOffset(68)]
        public fixed byte Name[32];

        public static MsgCharacter Create(in PixelEntity ntt)
        {
            ref var bdy = ref ntt.Get<BodyComponent>();
            ref var lvl = ref ntt.Get<LevelComponent>();
            ref var exp = ref ntt.Get<ExperienceComponent>();
            ref var pkp = ref ntt.Get<PkPointComponent>();
            ref var hlt = ref ntt.Get<HealthComponent>();
            ref var mna = ref ntt.Get<ManaComponent>();
            ref var pro = ref ntt.Get<ProfessionComponent>();
            ref var ntc = ref ntt.Get<NameTagComponent>();
            ref var mar = ref ntt.Get<MarriageComponent>();
            ref var inv = ref ntt.Get<InventoryComponent>();
            ref var atr = ref ntt.Get<AttributeComponent>();
            ref var rbn = ref ntt.Get<RebornComponent>();

            var spouseName = "None";
            // ref readonly var partner = ref PixelWorld.GetEntity(mar.SpouseId);
            // ref var sNtc = ref partner.Get<NameTagComponent>();
            // sNtc.Name ??= "None";

            var look = bdy.Look;
            look = (uint)(bdy.FaceId * 10_000 + bdy.Look);

            if (atr.Statpoints == 0)
            {
                atr = new AttributeComponent(ntt.Id)
                {
                    Agility = 10,
                    Strength = 10,
                    Vitality = 10,
                    Spirit = 10,
                    Statpoints = 10
                };
                ntt.Add(ref atr);
            }

            var packet = new MsgCharacter
            {
                Size = (ushort)(sizeof(MsgCharacter) - 30 + ntc.Name.Length + spouseName.Length),
                Id = 1006,
                EntityId = ntt.NetId,
                Look = look,
                Hair = bdy.Hair,
                Money = inv.Money,
                CPs = inv.CPs,
                Experience = exp.Experience,
                Strength = atr.Strength,
                Agility = atr.Agility,
                Vitality = atr.Vitality,
                Spirit = atr.Spirit,
                Statpoints = atr.Statpoints,
                Health = (ushort)hlt.Health,
                Mana = mna.Mana,
                PkPoints = pkp.Points,
                Level = lvl.Level,
                Class = (byte)pro.Class,
                Reborn = rbn.Count,
                ShowName = true,
                StringCount = 2,
            };

            packet.Name[0] = (byte)ntc.Name.Length;
            for (int i = 0; i < ntc.Name.Length; i++)
                packet.Name[i + 1] = (byte)ntc.Name[i];

            packet.Name[1 + ntc.Name.Length] = (byte)spouseName.Length;
            for (int i = 0; i < spouseName.Length; i++)
                packet.Name[ntc.Name.Length + 2 + i] = (byte)spouseName[i];
            return packet;
        }
    }
}