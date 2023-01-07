using MagnumOpus.Enums;

namespace MagnumOpus.Squiggly
{
    public readonly struct CqMap
    {
        public readonly ushort Id;
        public readonly ushort MapDocId;
        public readonly MapFlags Flags;
        public readonly string Name;
        public readonly Tuple<ushort, ushort, ushort> RespawnLocation;
        public readonly ushort Width;
        public readonly ushort Height;
        public readonly Dictionary<ushort, CqPortal> Portals;

        public CqMap(ushort id, ushort mapDocId, MapFlags flags, string name, Tuple<ushort, ushort, ushort> respawnLocation, ushort width, ushort height, Dictionary<ushort, CqPortal> portals)
        {
            Id = id;
            MapDocId = mapDocId;
            Flags = flags;
            Name = name;
            RespawnLocation = respawnLocation;
            Width = width;
            Height = height;
            Portals = portals;
        }
    }
}