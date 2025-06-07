using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles portal teleportation by finding portal links between maps and creating teleport components.
    /// Validates portal chains through map portals, passways, and exit destinations with backup fallback.
    /// </summary>
    public sealed class PortalSystem : NttSystem<PortalComponent, PositionComponent>
    {
        /// <summary>
        /// Initializes the PortalSystem with limited threading for portal processing.
        /// </summary>
        public PortalSystem() : base("Portal", threads: 1, log: false) { }

        /// <summary>
        /// Processes portal requests by finding destination coordinates through portal database lookups.
        /// </summary>
        /// <param name="ntt">The entity using the portal</param>
        /// <param name="portalComponent">Portal component containing target coordinates</param>
        /// <param name="position">Position component for current location</param>
        public override void Update(in NTT ntt, ref PortalComponent portalComponent, ref PositionComponent position)
        {
            var currentMapId = position.Map;
            var targetX = portalComponent.X;
            var targetY = portalComponent.Y;

            var portalEntry = Collections.DmapPortals.FirstOrDefault(p => p.MapId == currentMapId && Math.Abs(p.X - targetX) < 5 && Math.Abs(p.Y - targetY) < 5);
            if (portalEntry == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Dmap Portal found at {0}, {1} on map {2}", portalComponent.X, portalComponent.Y, currentMapId);
                ntt.Remove<PortalComponent>();

                var backupTeleport = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTeleport);
                return;
            }

            var passway = Collections.CqPassway.FirstOrDefault(x => x.mapid == currentMapId && x.passway_idx == portalEntry.PortalId);
            if (passway == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Passway found for {0} on map {1}", portalEntry.PortalId, currentMapId);
                ntt.Remove<PortalComponent>();

                var backupTeleport = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTeleport);
                return;
            }

            var exitPortal = Collections.CqPortal.FirstOrDefault(x => x.MapId == passway.passway_mapid && x.IdX == passway.passway_mapportal);
            if (exitPortal == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Exit Portal for {0} on map {1}", passway.passway_mapid, passway.passway_mapportal);
                ntt.Remove<PortalComponent>();

                var backupTeleport = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTeleport);
                return;
            }

            var teleportComponent = new TeleportComponent((ushort)exitPortal.X, (ushort)exitPortal.Y, (ushort)exitPortal.MapId);
            ntt.Set(ref teleportComponent);

            if (IsLogging)
                FConsole.WriteLine("Teleporting {0} to {1} at {2}, {3}", ntt.Id, exitPortal.MapId, exitPortal.X, exitPortal.Y);

            ntt.Remove<PortalComponent>();
        }
    }
}