using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

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

        public static MsgSpawn Create(in NTT ntt)
        {
            return ntt.Type switch
            {
                EntityType.Player => CreatePlayer(ntt),
                EntityType.Monster => CreateMonster(ntt),
                EntityType.Npc => throw new NotImplementedException(),
                EntityType.Item => throw new NotImplementedException(),
                EntityType.Trap => throw new NotImplementedException(),
                EntityType.Other => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public static MsgSpawn CreatePlayer(in NTT ntt)
        {
            ref readonly var hed = ref ntt.Get<HeadComponent>();
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var eqc = ref ntt.Get<EquipmentComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var look = bdy.Look;
            look = (uint)(hed.FaceId * 10_000 + bdy.Look);
            if (ntt.Has<DeathTagComponent>())
            {
                if (bdy.Look % 10000 == 2001 || bdy.Look % 10000 == 2002)
                    look = AddTransform(bdy.Look,99);
                else
                    look = AddTransform(bdy.Look, 98);
            }

            var head =0;
            var armor = 0;
            var mainHand = 0;
            var offHand =0;

            if(eqc.EntityId == ntt.Id)
            {
                eqc.Items.TryGetValue(MsgItemPosition.Head, out var headItem);
                head = headItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.Armor, out var armorItem);
                armor = armorItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.RightWeapon, out var mainHandItem);
                mainHand = mainHandItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.LeftWeapon, out var offHandItem);
                offHand = offHandItem.Get<ItemComponent>().Id;
            }

            
            var msg = new MsgSpawn
            {
                Size = (ushort)sizeof(MsgSpawn),
                Id = 1014,
                UniqueId = ntt.NetId,
                Look = look,
                StatusEffects = ntt.Get<StatusEffectComponent>().Effects,
                GuildRank = gld.Rank,
                Head = head,
                Armor = armor,
                MainHand = mainHand,
                OffHand = offHand,
                CurrentHp = (ushort)ntt.Get<HealthComponent>().Health,
                Level = ntt.Get<LevelComponent>().Level,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Hair = hed.Hair,
                Direction = bdy.Direction,
                Emote = ntt.Get<EmoteComponent>().Emote,
                Reborn = ntt.Get<RebornComponent>().Count,
                GuildId = (ushort)gld.GuildId,
                StringCount = 1,
                NameLen = (byte)ntc.Name.Length,
            };

            for (byte i = 0; i < ntc.Name.Length; i++)
                msg.Name[i] = (byte)ntc.Name[i];
            return msg;
        }

        public static MsgSpawn CreateMonster(in NTT ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var eff = ref ntt.Get<StatusEffectComponent>();

            if(string.IsNullOrEmpty(ntc.Name))
                ntc.Name = "";

            var msg = new MsgSpawn
            {
                Size = (ushort)sizeof(MsgSpawn),
                Id = 1014,
                UniqueId = ntt.NetId,
                Look = bdy.Look,
                StatusEffects = eff.Effects,
                CurrentHp = (ushort)hlt.Health,
                Level = lvl.Level,
                Direction = bdy.Direction,
                Emote = Emote.Stand,
                StringCount = 1,
                NameLen = (byte)ntc.Name.Trim().Length,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
            };
            for (byte i = 0; i < ntc.Name.Trim().Length; i++)
                msg.Name[i] = (byte)ntc.Name.Trim()[i];

            return msg;
        }
        
        public static uint AddTransform(uint look, long transformId) => (uint)(transformId * 10000000L + look % 10000000L);
        public static uint DelTransform(uint look) => look % 10000000;
    }
}