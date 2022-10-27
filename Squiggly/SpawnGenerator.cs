namespace MagnumOpus.Squiggly
{
    public readonly struct CqSpawnGenerator
    {
        public readonly int MapId;
        public readonly int MobId;
        public readonly int BornX;
        public readonly int BornY;
        public readonly int TimerBegin;
        public readonly int TimerEnd;
        public readonly int MaxAmount;
        public readonly int Xstart;
        public readonly int Ystart;
        public readonly int Xend;
        public readonly int Yend;
        public readonly int RespawnDelay;
        public readonly int Amount;

        public CqSpawnGenerator(int mapId, int mobId, int bornX, int bornY, int timerBegin, int timerEnd, int maxAmount, int xstart, int ystart, int xend, int yend, int respawnDelay, int amount)
        {
            MapId = mapId;
            MobId = mobId;
            BornX = bornX;
            BornY = bornY;
            TimerBegin = timerBegin;
            TimerEnd = timerEnd;
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