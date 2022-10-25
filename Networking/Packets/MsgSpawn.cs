using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 101*/)]
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

        public static Memory<byte> Create(PixelEntity ntt)
        {
            return ntt.Type switch
            {
                EntityType.Player => (Memory<byte>)CreatePlayer(ntt),
                EntityType.Monster => (Memory<byte>)CreateMonster(ntt),
                _ => default,
            };
        }

        public static byte[] CreatePlayer(PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<BodyComponent>();
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var eqc = ref ntt.Get<EquipmentComponent>();

            if (ntt.Has<DeathTagComponent>())
            {
                if (phy.Look % 10000 == 2001 || phy.Look % 10000 == 2002)
                    AddTransform(phy.Look,99);
                else
                    AddTransform(phy.Look, 98);
            }

            var packet = stackalloc MsgSpawn[1];
            packet->Size = (ushort)sizeof(MsgSpawn);
            packet->Id = 1014;
            packet->UniqueId = ntt.Id;
            packet->Look = phy.Look;
            packet->StatusEffects = phy.StatusEffects;
            packet->GuildRank = gld.Rank;
            packet->Head = eqc.Head;
            packet->Armor = eqc.Armor;
            packet->MainHand = eqc.MainHand;
            packet->OffHand = eqc.OffHand;
            packet->CurrentHp = hlt.Health;
            packet->Level = lvl.Level;
            packet->X = (ushort)phy.Location.X;
            packet->Y = (ushort)phy.Location.Y;
            packet->Hair = phy.Hair;
            packet->Direction = phy.Direction;
            packet->Emote = phy.Emote;
            packet->Reborn = phy.Reborn;
            packet->GuildId = (ushort)gld.GuildId;
            packet->StringCount = 1;

            packet->NameLen = (byte)ntc.Name.Length;
            for (byte i = 0; i < ntc.Name.Length; i++)
                packet->Name[i] = (byte)ntc.Name[i];

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgSpawn));
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *packet;
            return buffer;
        }

        public static byte[] CreateMonster(PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<BodyComponent>();
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();

            var packet = stackalloc MsgSpawn[1];
            packet->Size = (ushort)sizeof(MsgSpawn);
            packet->Id = 1014;
            packet->UniqueId = ntt.Id;
            packet->Look = phy.Look;
            packet->StatusEffects = phy.StatusEffects;
            packet->CurrentHp = hlt.Health;
            packet->Level = lvl.Level;
            packet->Direction = phy.Direction;
            packet->Emote = Emote.Stand;
            packet->StringCount = 1;
            packet->NameLen = (byte)ntc.Name.Trim().Length;
            packet->X = (ushort)phy.Location.X;
            packet->Y = (ushort)phy.Location.Y;
            for (byte i = 0; i < ntc.Name.Trim().Length; i++)
                packet->Name[i] = (byte)ntc.Name.Trim()[i];

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgSpawn));
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *packet;
            return buffer;
        }
        public static uint AddTransform(uint look, long transformId)
        {
            return (uint)(transformId * 10000000L + look % 10000000L);
        }

        public static uint DelTransform(uint look)
        {
            return look % 10000000;
        }

        public static implicit operator byte[](MsgSpawn msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgSpawn));
            fixed (byte* p = buffer)
                *(MsgSpawn*)p = *&msg;

                var tqServerBytes = Encoding.ASCII.GetBytes("TQServer");
            Array.Copy(tqServerBytes, 0, buffer, buffer.Length-tqServerBytes.Length, tqServerBytes.Length);
            
            return buffer;
        }
    }
}