using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public uint Look;
        public StatusEffect StatusEffects;
        public ushort GuildId;
        public readonly byte Unknown1;
        public GuildRanks GuildRank;
        public readonly int Garment;
        public int Head;
        public int Armor;
        public int MainHand;
        public int OffHand;
        public readonly int Unkown2;
        public ushort CurrentHp;
        public ushort Level;
        public ushort X;
        public ushort Y;
        public ushort Hair;
        public Direction Direction;
        public Emote Emote;
        public short Reborn;
        public readonly short Level2;
        public readonly int Unknown4;
        public readonly int NobilityRank;
        public readonly int UniqueId2;
        public readonly int NobilityPosition;
        public byte StringCount;
        public byte NameLen;
        public fixed byte Name[16];

        public static Memory<byte> Create(in PixelEntity ntt)
        {
            return ntt.Type switch
            {
                EntityType.Player => CreatePlayer(ntt),
                EntityType.Monster => CreateMonster(ntt)
            };
        }

        public static Memory<byte> CreatePlayer(in PixelEntity ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var eqc = ref ntt.Get<EquipmentComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var rbn = ref ntt.Get<RebornComponent>();
            ref readonly var eff = ref ntt.Get<StatusEffectComponent>();
            ref readonly var dir = ref ntt.Get<DirectionComponent>();

            if (ntt.Has<DeathTagComponent>())
            {
                if (bdy.Look % 10000 == 2001 || bdy.Look % 10000 == 2002)
                    AddTransform(bdy.Look,99);
                else
                    AddTransform(bdy.Look, 98);
            }

            var packet = stackalloc MsgSpawn[1];
            packet->Size = (ushort)sizeof(MsgSpawn);
            packet->Id = 1014;
            packet->UniqueId = ntt.NetId;
            packet->Look = bdy.Look;
            packet->StatusEffects = eff.Effects;
            packet->GuildRank = gld.Rank;
            packet->Head = eqc.Head;
            packet->Armor = eqc.Armor;
            packet->MainHand = eqc.MainHand;
            packet->OffHand = eqc.OffHand;
            packet->CurrentHp = hlt.Health;
            packet->Level = lvl.Level;
            packet->X = (ushort)pos.Position.X;
            packet->Y = (ushort)pos.Position.Y;
            packet->Hair = bdy.Hair;
            packet->Direction = dir.Direction;
            packet->Emote = bdy.Emote;
            packet->Reborn = rbn.Count;
            packet->GuildId = (ushort)gld.GuildId;
            packet->StringCount = 1;

            packet->NameLen = (byte)ntc.Name.Length;
            for (byte i = 0; i < ntc.Name.Length; i++)
                packet->Name[i] = (byte)ntc.Name[i];

            var buffer = new byte[sizeof(MsgSpawn)];
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *packet;
            return buffer;
        }

        public static Memory<byte> CreateMonster(in PixelEntity ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var eff = ref ntt.Get<StatusEffectComponent>();
            ref readonly var dir = ref ntt.Get<DirectionComponent>();

            var packet = stackalloc MsgSpawn[1];
            packet->Size = (ushort)sizeof(MsgSpawn);
            packet->Id = 1014;
            packet->UniqueId = ntt.NetId;
            packet->Look = bdy.Look;
            packet->StatusEffects = eff.Effects;
            packet->CurrentHp = hlt.Health;
            packet->Level = lvl.Level;
            packet->Direction = dir.Direction;
            packet->Emote = Emote.Stand;
            packet->StringCount = 1;
            packet->NameLen = (byte)ntc.Name.Trim().Length;
            packet->X = (ushort)pos.Position.X;
            packet->Y = (ushort)pos.Position.Y;
            for (byte i = 0; i < ntc.Name.Trim().Length; i++)
                packet->Name[i] = (byte)ntc.Name.Trim()[i];

            var buffer = new byte[sizeof(MsgSpawn)];
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *packet;
            return buffer;
        }
        
        public static uint AddTransform(uint look, long transformId) => (uint)(transformId * 10000000L + look % 10000000L);
        public static uint DelTransform(uint look) => look % 10000000;

        public static implicit operator Memory<byte>(MsgSpawn msg)
        {
            var buffer = new byte[sizeof(MsgSpawn)];
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *&msg;
            return buffer;
        }
    }
}