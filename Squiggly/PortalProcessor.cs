using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
#pragma warning disable CS8618
#pragma warning disable IDE1006
    public static class PortalProcessor
    {
        public static cq_portal FindPortal(int portalId, ushort mapid)
        {
            using var db = new SquigglyContext();
            cq_passway target = null;
            foreach (var cqPassway in db.cq_passway)
            {
                if (cqPassway.mapid != mapid)
                    continue;

                if (cqPassway.passway_idx == portalId)
                    target = cqPassway;
            }

            cq_portal portal = null;
            foreach (var cqportal in db.cq_portal)
            {
                if (cqportal.mapid != target.passway_mapid)
                    continue;
                if (cqportal.portal_idx == target.passway_mapportal)
                    portal = cqportal;
            }

            return portal;
        }
    }
}