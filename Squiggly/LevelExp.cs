namespace MagnumOpus.Squiggly
{
    public readonly struct CqLevelExp
    {
        public readonly int AllLevTime;
        public readonly byte Level;
        public readonly ulong ExpReq;

        public CqLevelExp(int allLevTime, byte level, ulong expReq)
        {
            AllLevTime = allLevTime;
            ExpReq = expReq;
            Level = level;
        }
    }
}