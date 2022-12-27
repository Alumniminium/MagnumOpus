using MagnumOpus.ECS;
using MagnumOpus.Components;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;
using HerstLib.IO;
using MagnumOpus.Networking.Packets;
using System.Numerics;

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
            if(entry == null)
            {
                FConsole.WriteLine($"PortalSystem: No Dmap Portal found at {ptc.X}, {ptc.Y} on map {mapId}");
                return;
            }

            var passway = Collections.CqPassway.FirstOrDefault(x => x.mapid == mapId && x.passway_idx == entry.PortalId);
            if(passway == null)
            {
                FConsole.WriteLine($"PortalSystem: No Passway for {entry.PortalId} on map {mapId}");
                return;
            }

            var exit = Collections.CqPortal.FirstOrDefault(x => x.MapId == passway.passway_mapid && x.IdX == passway.passway_mapportal);
            if(exit == null)
            {
                FConsole.WriteLine($"PortalSystem: No Exit Portal for {passway.passway_mapid} on map {passway.passway_mapportal}");
                return;
            }

            pos.Position = new Vector2(exit.X, exit.Y);
            pos.Map = exit.MapId;
            pos.ChangedTick = PixelWorld.Tick;

            FConsole.WriteLine($"PortalSystem: Teleported {ntt.NetId} to {exit.MapId} at {exit.X}, {exit.Y}");

            var tpP = MsgAction.Create(ntt.NetId, exit.MapId, (ushort)exit.X,(ushort)exit.Y,Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref tpP);
            var mapStatus = MsgStatus.Create((uint)exit.MapId, (uint)Enums.MapFlags.None);
            ntt.NetSync(in mapStatus);

            ntt.Remove<PortalComponent>();
        }
    }
}