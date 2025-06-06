namespace MagnumOpus.Squiggly
{
    public class CqPortal(int mapId, int x, int y, int id, long idx)
    {
        public readonly int MapId = mapId;
        public readonly int X = x;
        public readonly int Y = y;
        public readonly int Id = id;
        public readonly long IdX = idx;
    }
}