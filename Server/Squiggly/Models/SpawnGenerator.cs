namespace MagnumOpus.Squiggly
{
    public readonly struct CqSpawnGenerator(int mapId, int mobId, int maxAmount, int xstart, int ystart, int xend, int yend, int respawnDelay, int amount)
    {
        public readonly int MapId = mapId;
        public readonly int MobId = mobId;
        public readonly int MaxAmount = maxAmount;
        public readonly int Xstart = xstart;
        public readonly int Ystart = ystart;
        public readonly int Xend = xend;
        public readonly int Yend = yend;
        public readonly int RespawnDelay = respawnDelay;
        public readonly int Amount = amount;
    }
}