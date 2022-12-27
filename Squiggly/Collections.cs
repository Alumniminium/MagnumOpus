namespace MagnumOpus.Squiggly
{
    public static class Collections
    {
        public static Dictionary<int, CqPortal> Portals { get; set; } = new();
        public static Dictionary<int, CqMap> Maps { get; set; } = new();
        public static Dictionary<int, CqMonster> BaseMonsters { get; set; } = new();
        public static Dictionary<int, CqMonster> Monsters { get; set; } = new();
        public static Dictionary<int, CqSpawnGenerator> Spawns { get; set; } = new();
        public static Dictionary<int, CqLevelExp> LevelExps { get; set; } = new();
        public static Dictionary<int, CqItemBonus> ItemBonus { get; set; } = new();
    }
}