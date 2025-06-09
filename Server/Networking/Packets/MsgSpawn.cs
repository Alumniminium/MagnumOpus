using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Networking.Packets
{
    /// <summary>
    /// Network packet for spawning entities (players, monsters, NPCs) in the game world.
    /// Contains complete entity appearance data including equipment, position, status effects, and visual transforms.
    /// Supports different entity types with specialized creation methods for players and monsters.
    /// </summary>
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

        /// <summary>
        /// Creates a spawn packet for any entity type by routing to the appropriate specialized creation method.
        /// </summary>
        /// <param name="ntt">Entity to create spawn packet for</param>
        /// <returns>Configured spawn packet for the entity</returns>
        public static MsgSpawn Create(in NTT ntt)
        {
            if (ntt.IsPlayer())
                return CreatePlayer(ntt);
            else if (ntt.IsMonster(guardsAreMonsters: true))
                return CreateMonster(ntt);
            else
                throw new NotImplementedException("Unknown entity type");
        }

        /// <summary>
        /// Creates a spawn packet specifically for player entities with complete equipment and appearance data.
        /// Handles death transforms, equipment visualization, and guild information display.
        /// </summary>
        /// <param name="ntt">Player entity to create spawn packet for</param>
        /// <returns>Player spawn packet with equipment, stats, and appearance data</returns>
        public static MsgSpawn CreatePlayer(in NTT ntt)
        {
            ref readonly var hed = ref ntt.Get<HeadComponent>();
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref var ntc = ref ntt.Get<NameTagComponent>();
            ref readonly var gld = ref ntt.Get<GuildComponent>();
            ref readonly var eqc = ref ntt.Get<EquipmentComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var look = bdy.Look == 0 ? 1003 : bdy.Look;
            if (hed.FaceId != 0)
                look = (uint)((hed.FaceId * 10_000) + bdy.Look);
            if (ntt.Has<DeathTagComponent>())
                look = bdy.Look % 10000 is 2001 or 2002 ? AddTransform(bdy.Look, 99) : AddTransform(bdy.Look, 98);

            var head = 0;
            var armor = 0;
            var mainHand = 0;
            var offHand = 0;

            if (eqc.Items is not null)
            {
                eqc.Items.TryGetValue(MsgItemPosition.Head, out var headItem);
                head = headItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.Armor, out var armorItem);
                armor = armorItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.PrimaryWeapon, out var mainHandItem);
                mainHand = mainHandItem.Get<ItemComponent>().Id;
                eqc.Items.TryGetValue(MsgItemPosition.SecondaryWeapon, out var offHandItem);
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
                Direction = pos.Direction,
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

        /// <summary>
        /// Creates a spawn packet specifically for monster entities with basic appearance and name data.
        /// Looks up monster names from the database and configures basic visual properties.
        /// </summary>
        /// <param name="ntt">Monster entity to create spawn packet for</param>
        /// <returns>Monster spawn packet with basic appearance and name data</returns>
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

            name = !Collections.CqMonsterType.TryGetValue(cqm.CqMonsterId, out var cqMob) ? "Id:" + ntt.Id : cqMob.name;

            var msg = new MsgSpawn
            {
                Size = (ushort)sizeof(MsgSpawn),
                Id = 1014,
                UniqueId = ntt.Id,
                Look = bdy.Look,
                StatusEffects = eff.Effects,
                CurrentHp = (ushort)hlt.Health,
                Level = lvl.Level,
                Direction = pos.Direction,
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

        /// <summary>
        /// Adds a visual transformation effect to an entity's appearance (death, special effects, etc.).
        /// </summary>
        /// <param name="look">Base appearance value</param>
        /// <param name="transformId">Transform effect identifier</param>
        /// <returns>Modified appearance value with transform applied</returns>
        public static uint AddTransform(uint look, long transformId) => (uint)((transformId * 10000000L) + (look % 10000000L));

        /// <summary>
        /// Removes visual transformation effects from an entity's appearance, returning to base look.
        /// </summary>
        /// <param name="look">Appearance value with potential transforms</param>
        /// <returns>Base appearance value without transforms</returns>
        public static uint DelTransform(uint look) => look % 10000000;
    }
}