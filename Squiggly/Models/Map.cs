using MagnumOpus.Enums;

namespace MagnumOpus.Squiggly.Models
{
    public readonly struct CqMap
    {
        public readonly ushort Id;
        public readonly ushort MapDocId;
        public readonly MapFlags Flags;
        public readonly string Name;
        public readonly ValueTuple<ushort, ushort, ushort> RespawnLocation;
        public readonly ushort Width;
        public readonly ushort Height;
        public readonly Dictionary<ushort, CqPortal> Portals;

        public CqMap(ushort id, ushort mapDocId, MapFlags flags, string name, ValueTuple<ushort, ushort, ushort> respawnLocation, ushort width, ushort height, Dictionary<ushort, CqPortal> portals)
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

        public override string ToString()
        {
            var portalsString = string.Join(", ", Portals.Select(p => $"Id: {p.Value.Id}"));
            var respawnString = $"X: {RespawnLocation.Item1}, Y: {RespawnLocation.Item2}, Z: {RespawnLocation.Item3}";

            return $"Map:" + Environment.NewLine +
                $"  Id: {Id}" + Environment.NewLine +
                $"  MapDocId: {MapDocId}" + Environment.NewLine +
                $"  Name: {Name}" + Environment.NewLine +
                $"  RespawnLocation: {respawnString}" + Environment.NewLine +
                $"  Width: {Width}" + Environment.NewLine +
                $"  Height: {Height}" + Environment.NewLine +
                $"  Portals: {portalsString}";
        }
    }
}