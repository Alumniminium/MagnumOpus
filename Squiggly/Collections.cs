using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
    public static class Collections
    {
        public static HashSet<CqPortal> CqPortal { get; set; } = new();
        public static HashSet<Dmap_Portals> DmapPortals { get; set; } = new();
        public static HashSet<cq_passway> CqPassway { get; set; } = new();
        public static Dictionary<int, CqMap> Maps { get; set; } = new();
        public static Dictionary<int, CqMonster> BaseMonsters { get; set; } = new();
        public static Dictionary<int, CqMonster> Monsters { get; set; } = new();
        public static Dictionary<int, CqSpawnGenerator> Spawns { get; set; } = new();
        public static Dictionary<int, CqLevelExp> LevelExps { get; set; } = new();
        public static Dictionary<int, CqItemBonus> ItemBonus { get; set; } = new();
    }
}