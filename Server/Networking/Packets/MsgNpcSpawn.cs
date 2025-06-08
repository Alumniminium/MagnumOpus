using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.IO;
using NttECS.ECS;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 101)]
    public unsafe struct MsgNpcSpawn
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public ushort X;
        public ushort Y;
        public ushort Look;
        public NpcType NpcType;
        public NpcSort Sort;
        public ushort Base;

        public static MsgNpcSpawn Create(in NTT ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var npc = ref ntt.Get<NpcComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var packet = new MsgNpcSpawn
            {
                Size = (ushort)sizeof(MsgNpcSpawn),
                Id = 2030,
                UniqueId = ntt.Id,
                Look = (ushort)bdy.Look,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                NpcType = npc.Type,
                Base = npc.Base,
                Sort = npc.Sort
            };

            FConsole.WriteLine($"MsgNpcSpawn: {packet.X}, {packet.Y}:");
            FConsole.WriteLine($"MsgNpcSpawn: Type: {packet.NpcType} - {Enum.GetName(typeof(NpcType), packet.NpcType)}");
            FConsole.WriteLine($"MsgNpcSpawn: Sort: {packet.Sort} - {Enum.GetName(typeof(NpcSort), packet.Sort)}");
            FConsole.WriteLine($"MsgNpcSpawn: Base: {packet.Base} - {Enum.GetName(typeof(NpcSort), packet.Base)}");

            return packet;
        }
    }
    public enum NpcSort : ushort
    {
        None = 0,
        Task = 1,           // Task type (任务类)
        Recycle = 2,        // Recyclable type (可回收类)
        Scene = 4,          // Scene type (with map objects) (场景类(带地图物件))
        LinkMap = 8,        // Link map type (LINKID is map ID, mutually exclusive with others using LINKID) (挂地图类(LINKID为地图ID，与其他使用LINKID的互斥))
        DieAction = 16,     // With death task (LINKID is ACTION_ID, mutually exclusive with others using LINKID) (带死亡任务(LINKID为ACTION_ID，与其他使用LINKID的互斥))
        DelEnable = 32,     // Can be manually deleted (not through tasks) (可以手动删除(不是指通过任务))
        Event = 64,         // With timed task, time in data3, format MMWWHHMMSS (LINKID is ACTION_ID, mutually exclusive with others using LINKID) (带定时任务, 时间在data3中，格式为MMWWHHMMSS。(LINKID为ACTION_ID，与其他使用LINKID的互斥))
        Table = 128,        // With data table type (带数据表类型)

        //      NPCSORT_SHOP        = ,            // Shop type (商店类)
        //      NPCSORT_DICE        = ,            // Dice NPC (骰子NPC)

        NpcsortUseLinkId = LinkMap | DieAction | Event,
    };
    public enum NpcType : ushort
    {
        ROLE_NPC_NONE = 0, // Illegal NPC
        ROLE_SHOPKEEPER_NPC = 1, // Shop NPC
        ROLE_TASK_NPC = 2, // Task NPC
        ROLE_STORAGE_NPC = 3, // Storage NPC
        ROLE_TRUNCK_NPC = 4, // Box NPC
        ROLE_FORGE_NPC = 6, // Forging NPC
        ROLE_EMBED_NPC = 7, // Embedding NPC
        ROLE_COMPOSE_NPC = 8, // Qiankun Five Elements Furnace
        ROLE_STATUARY_NPC = 9, // Statue NPC
        ROLE_SYNFLAG_NPC = 10, // Gang Mark NPC
        ROLE_PLAYER = 11, // Other players
        ROLE_HERO = 12, // Yourself
        ROLE_MONSTER = 13, // Monster
        ROLE_BOOTH_NPC = 14, // Stall NPC
        SYNTRANS_NPC = 15, // Gang Teleport NPC (used for 00:00 charging) (LINKID is the ID of the fixed NPC, mutually exclusive with other LINKID)
        ROLE_BOOTH_FLAG_NPC = 16, // Stall Flag NPC
        ROLE_MOUSE_NPC = 17, // NPC on the mouse
        ROLE_MAGICITEM = 18, // Trap Fire Wall
        ROLE_DICE_NPC = 19, // Dice NPC
        ROLE_WEAPONGOAL_NPC = 21, // Melee Attack NPC
        ROLE_MAGICGOAL_NPC = 22, // Magic Attack Target NPC
        ROLE_BOWGOAL_NPC = 23, // Bow and Arrow Target NPC
        ROLE_TARGET_NPC = 24, // Take a beating, no quest triggered
        ROLE_FURNITURE_NPC = 25, // Furniture NPC
        ROLE_CITY_GATE_NPC = 26, // City Gate NPC
        ROLE_NEIGHBOR_DOOR = 27, // Neighbor's Door
        ROLE_CALL_PET = 28, // Summoned Beast
        ROLE_TELEPORT = 29, // Teleport NPC
        ROLE_MOUNT_APPEND = 30, // Mount Pet Combination NPC
        ROLE_FAMILY_OCCUPY_NPC = 31,
        TASK_SHOPKEEPER_NPC = 32, // Quest Shop NPC
        TASK_FORGE_NPC = 33, // Quest Forging NPC
        TASK_EMBED_NPC = 34, // Quest Embedding NPC
        COMPOSE_GEM_NPC = 35, // Gem Combination NPC
        REDUCE_DMG_NPC = 36, // Equipment God Blessing NPC
        MAKE_ITEM_HOLE_NPC = 37, // Item Hole NPC
        SOLIDIFY_ITEM_NPC = 38, // Solidify equipment NPC
        COMPETE_BARRIER_NPC_ = 39, // Mount pet competition fence NPC
        FACTION_MATCH_FLAG = 40, // Gang battle flag
        FM_LEFT_BARRIER_NPC_ = 41, // Gang battle left base camp fence
        FM_RIGHT_BARRIER_NPC_ = 42, // Gang battle right base camp fence
        WARFLAG_FLAGALTAR = 43, // Cross-server battle flag competition battle flag platform
        WARFLAG_PRESENTFLAG = 44, // Cross-server battle flag competition flag-giving NPC
        WARFLAG_FLAG = 45, // Cross-server battle flag competition battle flag
        VEXILLUM_FLAGALTAR = 46, // New battle flag competition flag platform (the cut battle flag)
        VEXILLUM_FLAG = 47, // New Battle Flag Competition Flag (small trap-type flag)
        SLOT_MACHINE_NPC = 60, // Slot Machine NPC
        OS_LANDLORD = 61, // Cross-server players can attack NPC
        CHANGE_LOOKFACE_TASK_NPC = 62, // Task NPC that can change lookface
        ROLE_DESTRUCTIBLE_NPC = 63, // Destructible NPC
        ROLE_SYNBUFF_NPC = 64, // Gang Buff Pillar NPC
        SYN_BOSS = 65, // Gang BOSS
        FURNITURE3D_NPC = 101, // 3D Furniture NPC
        CITY_WALL_NPC = 102, // City Wall NPC
        CITY_MOAT_NPC = 103, // Moat NPC
        TEXAS_TABLE_NPC = 110, // Gambling table NPC
        TRAP_MONSTER = 111, // Trap Monster
        ROULETTE_TABLE_NPC = 112, // Roulette table NPC
        FRONTIER_SERVER_TRANS_NPC = 113, // Border server transmission NPC
        TRAP_CAN_BE_ATTACK_NPC = 114, // Trap NPC that can be attacked
        SH_TABLE_NPC = 115, // Stud table NPC
        RAIDER_TABLE_NPC = 116, // Raiders of the Lost Ark table NPC
        TURRET_NPC = 117, // Turret NPC
        DOMINO_TABLE_NPC = 118, // Domino table NPC
        TRAP_TRAPSORT_PORTAL = 119, // Blue rune of waterway - instant portal
        NEWSLOT_NPC = 120, // 5*3 slot machine NPC
        BLACKJACK_TABLE_NPC = 121, // 21-point gambling table NPC
        FRUIT_MACHINE_NPC = 122, // Fruit machine NPC
        SHEN_DING_NPC = 123, // Shen Ding NPC
        SWORD_PRISON = 124, // Sword prison NPC
    }
}