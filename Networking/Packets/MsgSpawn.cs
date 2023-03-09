using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Guild;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Leveling;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;

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
                EntityType.InvItem => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public static MsgSpawn CreatePlayer(in NTT ntt)
        {
            ref readonly var hed = ref ntt.Get<HeadComponent>();
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var eqc = ref ntt.Get<EquipmentComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var look = bdy.Look;
            look = (uint)((hed.FaceId * 10_000) + bdy.Look);
            if (ntt.Has<DeathTagComponent>())
                look = bdy.Look % 10000 is 2001 or 2002 ? AddTransform(bdy.Look, 99) : AddTransform(bdy.Look, 98);

            var head = 0;
            var armor = 0;
            var mainHand = 0;
            var offHand = 0;

            if (eqc.Items != null)
            {
                _ = eqc.Items.TryGetValue(MsgItemPosition.Head, out var headItem);
                head = headItem.Get<ItemComponent>().Id;
                _ = eqc.Items.TryGetValue(MsgItemPosition.Armor, out var armorItem);
                armor = armorItem.Get<ItemComponent>().Id;
                _ = eqc.Items.TryGetValue(MsgItemPosition.RightWeapon, out var mainHandItem);
                mainHand = mainHandItem.Get<ItemComponent>().Id;
                _ = eqc.Items.TryGetValue(MsgItemPosition.LeftWeapon, out var offHandItem);
                offHand = offHandItem.Get<ItemComponent>().Id;
            }

            ntc.Name ??= "Id:" + ntt.Id;

            var msg = new MsgSpawn
            {
                Size = (ushort)sizeof(MsgSpawn),
                Id = 1014,
                UniqueId = ntt.Id,
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
            ref readonly var lvl = ref ntt.Get<LevelComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var cqm = ref ntt.Get<CqMonsterComponent>();
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var eff = ref ntt.Get<StatusEffectComponent>();
            var name = string.Empty;

            if (!Collections.CqMonsterType.TryGetValue(cqm.CqMonsterId, out var cqMob))
                name = "Id:" + ntt.Id;
            else
                name = cqMob.name;

            var msg = new MsgSpawn
            {
                Size = (ushort)sizeof(MsgSpawn),
                Id = 1014,
                UniqueId = ntt.Id,
                Look = bdy.Look,
                StatusEffects = eff.Effects,
                CurrentHp = (ushort)hlt.Health,
                Level = lvl.Level,
                Direction = bdy.Direction,
                Emote = Emote.Stand,
                StringCount = 1,
                NameLen = (byte)name.Trim().Length,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
            };
            for (byte i = 0; i < name.Trim().Length; i++)
                msg.Name[i] = (byte)name.Trim()[i];

            return msg;
        }

        public static uint AddTransform(uint look, long transformId) => (uint)((transformId * 10000000L) + (look % 10000000L));
        public static uint DelTransform(uint look) => look % 10000000;
    }
}