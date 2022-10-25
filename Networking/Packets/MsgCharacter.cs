using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

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

        public static Memory<byte> Create(PixelEntity entity)
        {
            ref var phy = ref entity.Get<BodyComponent>();
            ref var lvl = ref entity.Get<LevelComponent>();
            ref var pkp = ref entity.Get<PkPointComponent>();
            ref var hlt = ref entity.Get<HealthComponent>();
            ref var mna = ref entity.Get<ManaComponent>();
            ref var pro = ref entity.Get<ProfessionComponent>();
            ref var ntc = ref entity.Get<NameTagComponent>();
            ref var mar = ref entity.Get<MarriageComponent>();
            ref var inv = ref entity.Get<InventoryComponent>();
            ref var atr = ref entity.Get<AttributeComponent>();

            ref readonly var partner = ref PixelWorld.GetEntity(mar.SpouseId);
            ref var sNtc = ref partner.Get<NameTagComponent>();

            if(sNtc.Name == null)
            {
                sNtc.Name ="None";
            }
            if(ntc.Name == null)
            {
                ntc.Name = "None";
            }

            if(phy.Look == 0)
                phy.Look = 1003;
            if(phy.Emote == 0)
                phy.Emote = Enums.Emote.Stand;

            phy.Hair = 302;
            
            if(atr.Statpoints == 0)
            {
                atr.Agility = 10;
                atr.Strength = 10;
                atr.Vitality = 10;
                atr.Spirit = 10;
                atr.Statpoints = 10;
            }

            if(hlt.Health == 0)
            {
                hlt.Health = 100;
                hlt.MaxHealth = 100;
            }

            if(lvl.Level == 0)
            {
                lvl.Level = 1;
                lvl.Experience = 0;
            }

            if(mna.Mana == 0)
            {
                mna.Mana = 100;
                mna.MaxMana = 100;
            }
            
            var packet = new MsgCharacter
            {
                Size = (ushort)(sizeof(MsgCharacter) - 30 + ntc.Name.Length + sNtc.Name.Length),
                Id = 1006,
                EntityId = entity.Id,
                Look = phy.Look,
                Hair = phy.Hair,
                Money = inv.Money,
                CPs = inv.CPs,
                Experience = lvl.Experience,
                Strength = atr.Strength,
                Agility = atr.Agility,
                Vitality = atr.Vitality,
                Spirit = atr.Spirit,
                Statpoints = atr.Statpoints,
                Health = hlt.Health,
                Mana = mna.Mana,
                PkPoints = pkp.Points,
                Level = lvl.Level,
                Class = (byte)pro.Class,
                Reborn = phy.Reborn,
                ShowName = true,
                StringCount = 2,
            };

            packet.Name[0] = (byte)ntc.Name.Length;
            for (int i = 0; i < ntc.Name.Length; i++)
                packet.Name[i + 1] = (byte)ntc.Name[i];

            packet.Name[1 + ntc.Name.Length] = (byte)sNtc.Name.Length;
            for (int i = 0; i < sNtc.Name.Length; i++)
                packet.Name[ntc.Name.Length + 2 + i] = (byte)sNtc.Name[i];
            return packet;
        }

        public static implicit operator Memory<byte>(MsgCharacter packet)
        { 
            var buffer = new byte[packet.Size + 8];
            fixed (byte* p = buffer)
                *(MsgCharacter*)p = packet;
            
            var tqServerBytes = Encoding.ASCII.GetBytes("TQServer");
            Array.Copy(tqServerBytes, 0, buffer, buffer.Length-tqServerBytes.Length, tqServerBytes.Length);
            
            return buffer;
        }
    }
}