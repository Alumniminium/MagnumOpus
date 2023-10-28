using System.Collections.Concurrent;
using Co2Core.IO;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
    public static class Collections
    {
        internal static readonly Dictionary<int, SpatialHash> SpatialHashes = [];

        internal static ConcurrentDictionary<long, cq_action> CqAction = new();
        internal static ConcurrentDictionary<long, cq_task> CqTask = new();
        internal static ConcurrentDictionary<long, cq_npc> CqNpc = new();

        public static List<cq_point_allot> CqPointAllot { get; set; } = [];
        public static HashSet<CqPortal> CqPortal { get; set; } = [];
        public static HashSet<Dmap_Portals> DmapPortals { get; set; } = [];
        public static HashSet<cq_passway> CqPassway { get; set; } = [];
        public static Dictionary<int, CqMap> Maps { get; set; } = [];
        public static Dictionary<int, CqItemBonus> ItemBonus { get; set; } = [];
        public static LevelExp LevelExps { get; set; } = new();
        public static ItemType ItemType { get; set; } = new();
        public static MagicType MagicType { get; set; } = new();
        public static Dictionary<int, ShopDatEntry> Shops { get; set; } = [];
        public static ConcurrentDictionary<int, List<ItemType.Entry>> Drops { get; set; } = new();
        public static Dictionary<int, cq_monstertype> CqMonsterType { get; set; } = [];
    }
}