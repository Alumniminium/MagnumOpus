using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class PortalSystem : NttSystem<PortalComponent, PositionComponent>
    {
        public PortalSystem() : base("Portal", threads: 2) { }

        public override void Update(in NTT ntt, ref PortalComponent ptc, ref PositionComponent pos)
        {
            var mapId = pos.Map;
            var x = ptc.X;
            var y = ptc.Y;

            var entry = Collections.DmapPortals.FirstOrDefault(p => p.MapId == mapId && Math.Abs(p.X - x) < 5 && Math.Abs(p.Y - y) < 5);
            if (entry == null)
            {
                if (IsLogging)
                    Logger.Debug("No Dmap Portal found at {0}, {1} on map {2}", ptc.X, ptc.Y, mapId);
                ntt.Remove<PortalComponent>();

                var backupTpc = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTpc);
                return;
            }

            var passway = Collections.CqPassway.FirstOrDefault(x => x.mapid == mapId && x.passway_idx == entry.PortalId);
            if (passway == null)
            {
                if (IsLogging)
                    Logger.Debug("No Passway found for {0} on map {1}", entry.PortalId, mapId);
                ntt.Remove<PortalComponent>();

                var backupTpc = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTpc);
                return;
            }

            var exit = Collections.CqPortal.FirstOrDefault(x => x.MapId == passway.passway_mapid && x.IdX == passway.passway_mapportal);
            if (exit == null)
            {
                if (IsLogging)
                    Logger.Debug("No Exit Portal for {0} on map {1}", passway.passway_mapid, passway.passway_mapportal);
                ntt.Remove<PortalComponent>();

                var backupTpc = new TeleportComponent(477, 380, 1002);
                ntt.Set(ref backupTpc);
                return;
            }

            var tpc = new TeleportComponent((ushort)exit.X, (ushort)exit.Y, (ushort)exit.MapId);
            ntt.Set(ref tpc);

            if (IsLogging)
                Logger.Debug("Teleporting {0} to {1} at {2}, {3}", ntt.Id, exit.MapId, exit.X, exit.Y);

            ntt.Remove<PortalComponent>();
        }
    }
}