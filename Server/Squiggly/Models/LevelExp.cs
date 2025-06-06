namespace MagnumOpus.Squiggly
{
    public readonly struct CqLevelExp(int allLevTime, byte level, ulong expReq)
    {
        public readonly int AllLevTime = allLevTime;
        public readonly byte Level = level;
        public readonly ulong ExpReq = expReq;
    }
}