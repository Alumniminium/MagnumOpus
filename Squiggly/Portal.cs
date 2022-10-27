namespace MagnumOpus.Squiggly
{
    public readonly struct CqPortal
    {
        public readonly int MapId;
        public readonly int X;
        public readonly int Y;
        public readonly int Id;
        public readonly long IdX;

        public CqPortal(int mapId, int x, int y, int id, long idx)
        {
            MapId = mapId;
            X = x;
            Y = y;
            Id = id;
            IdX = idx;
        }
    }
}