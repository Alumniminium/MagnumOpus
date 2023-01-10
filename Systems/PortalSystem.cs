using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class PortalSystem : PixelSystem<PortalComponent, PositionComponent>
    {
        public PortalSystem() : base("Portal System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PortalComponent ptc, ref PositionComponent pos)
        {
            var mapId = pos.Map;
            var x = ptc.X;
            var y = ptc.Y;

            var entry = Collections.DmapPortals.FirstOrDefault(p => p.MapId == mapId && Math.Abs(p.X - x) < 5 && Math.Abs(p.Y - y) < 5);
            if (entry == null)
            {
                FConsole.WriteLine($"[{nameof(PortalSystem)}] {ntt.NetId} -> No Dmap Portal found at {ptc.X}, {ptc.Y} on map {mapId}");
                ntt.Remove<PortalComponent>();
                return;
            }

            var passway = Collections.CqPassway.FirstOrDefault(x => x.mapid == mapId && x.passway_idx == entry.PortalId);
            if (passway == null)
            {
                FConsole.WriteLine($"[{nameof(PortalSystem)}] {ntt.NetId} -> No Passway for {entry.PortalId} on map {mapId}");
                ntt.Remove<PortalComponent>();
                return;
            }

            var exit = Collections.CqPortal.FirstOrDefault(x => x.MapId == passway.passway_mapid && x.IdX == passway.passway_mapportal);
            if (exit == null)
            {
                FConsole.WriteLine($"[{nameof(PortalSystem)}] {ntt.NetId} -> No Exit Portal for {passway.passway_mapid} on map {passway.passway_mapportal}");
                ntt.Remove<PortalComponent>();
                return;
            }

            var tpc = new TeleportComponent(ntt.Id, (ushort)exit.X, (ushort)exit.Y, (ushort)exit.MapId);
            ntt.Set(ref tpc);

            ntt.Remove<PortalComponent>();
        }
    }
}