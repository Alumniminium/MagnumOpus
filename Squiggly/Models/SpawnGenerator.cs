namespace MagnumOpus.Squiggly
{
    public readonly struct CqSpawnGenerator
    {
        public readonly int MapId;
        public readonly int MobId;
        public readonly int MaxAmount;
        public readonly int Xstart;
        public readonly int Ystart;
        public readonly int Xend;
        public readonly int Yend;
        public readonly int RespawnDelay;
        public readonly int Amount;

        public CqSpawnGenerator(int mapId, int mobId,int maxAmount, int xstart, int ystart, int xend, int yend, int respawnDelay, int amount)
        {
            MapId = mapId;
            MobId = mobId;
            MaxAmount = maxAmount;
            Xstart = xstart;
            Ystart = ystart;
            Xend = xend;
            Yend = yend;
            RespawnDelay = respawnDelay;
            Amount = amount;
        }
    }
}