using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class PortalSystem : NttSystem<PortalComponent, PositionComponent>
    {
        public PortalSystem() : base("Portal", threads: 1, log: false) { }

        // Handles portal teleportation by finding portal links between maps and creating teleport
        // components. Validates portal chains through a 3-step lookup: map portals -> passways ->
        // exit destinations. Uses tolerance-based position matching (within 5 units) and provides
        // backup fallback teleportation to Twin City (477, 380, map 1002) if any lookup fails.
        public override void Update(in NTT ntt, ref PortalComponent portalComponent, ref PositionComponent position)
        {
            var currentMapId = position.Map;
            var targetX = portalComponent.X;
            var targetY = portalComponent.Y;

            // === STEP 1: FIND PORTAL ENTRY ON CURRENT MAP ===
            // Look for portal within 5 units of target coordinates
            var portalEntry = Collections.DmapPortals.FirstOrDefault(p =>
                p.MapId == currentMapId &&
                Math.Abs(p.X - targetX) < 5 &&
                Math.Abs(p.Y - targetY) < 5);

            if (portalEntry == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Dmap Portal found at {x}, {y} on map {map}", targetX, targetY, currentMapId);

                CreateBackupTeleport(ntt);
                return;
            }

            // === STEP 2: FIND PASSWAY FOR PORTAL ===
            // Look up the passway configuration for this portal
            var passway = Collections.CqPassway.FirstOrDefault(x =>
                x.mapid == currentMapId &&
                x.passway_idx == portalEntry.PortalId);

            if (passway == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Passway found for portal {portal} on map {map}", portalEntry.PortalId, currentMapId);

                CreateBackupTeleport(ntt);
                return;
            }

            // === STEP 3: FIND EXIT PORTAL ON DESTINATION MAP ===
            // Look up the exit portal coordinates on the destination map
            var exitPortal = Collections.CqPortal.FirstOrDefault(x =>
                x.MapId == passway.passway_mapid &&
                x.IdX == passway.passway_mapportal);

            if (exitPortal == null)
            {
                if (IsLogging)
                    FConsole.WriteLine("No Exit Portal for map {map} portal {portal}", passway.passway_mapid, passway.passway_mapportal);

                CreateBackupTeleport(ntt);
                return;
            }

            // === CREATE SUCCESSFUL TELEPORT ===
            var teleportComponent = new TeleportComponent(
                (ushort)exitPortal.X,
                (ushort)exitPortal.Y,
                (ushort)exitPortal.MapId);
            ntt.Set(ref teleportComponent);

            if (IsLogging)
                FConsole.WriteLine("{ntt} teleporting to map {map} at {x}, {y}", ntt, exitPortal.MapId, exitPortal.X, exitPortal.Y);

            ntt.Remove<PortalComponent>();
        }

        private static void CreateBackupTeleport(in NTT ntt)
        {
            // Teleport to Twin City as backup when portal lookup fails
            var backupTeleport = new TeleportComponent(477, 380, 1002);
            ntt.Set(ref backupTeleport);
            ntt.Remove<PortalComponent>();
        }
    }
}