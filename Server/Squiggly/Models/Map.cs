using MagnumOpus.Enums;

namespace MagnumOpus.Squiggly.Models
{
    public readonly struct CqMap(ushort id, ushort mapDocId, MapFlags flags, string name, ValueTuple<ushort, ushort, ushort> respawnLocation, ushort width, ushort height, Dictionary<ushort, CqPortal> portals)
    {
        public readonly ushort Id = id;
        public readonly ushort MapDocId = mapDocId;
        public readonly MapFlags Flags = flags;
        public readonly string Name = name;
        public readonly ValueTuple<ushort, ushort, ushort> RespawnLocation = respawnLocation;
        public readonly ushort Width = width;
        public readonly ushort Height = height;
        public readonly Dictionary<ushort, CqPortal> Portals = portals;

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