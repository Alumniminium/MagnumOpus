using System.Collections.Concurrent;
using Co2Core.IO;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
    public static class Collections
    {
        public static HashSet<CqPortal> CqPortal { get; set; } = new();
        public static HashSet<Dmap_Portals> DmapPortals { get; set; } = new();
        public static HashSet<cq_passway> CqPassway { get; set; } = new();
        public static Dictionary<int, CqMap> Maps { get; set; } = new();
        public static Dictionary<int, CqItemBonus> ItemBonus { get; set; } = new();
        public static LevelExp LevelExps { get; set; } = new();
        public static ItemType ItemType { get; set; } = new();
        public static MagicType MagicType { get; set; } = new();
        public static Dictionary<int, ShopDatEntry> Shops { get; set; } = new();
        public static ConcurrentDictionary<int, List<ItemType.Entry>> Drops { get;set; } = new ();
    }
}