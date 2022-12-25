using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
    public static class PortalProcessor
    {
        public static cq_portal? FindPortal(int portalId, ushort mapid)
        {
            using var db = new SquigglyContext();
            cq_passway? target = null;
            cq_portal? portal = null;

            foreach (var cqPassway in db.cq_passway.Where(x => x.passway_mapid == mapid))
            {
                if (cqPassway.passway_idx == portalId)
                    target = cqPassway;
            }

            if (target == null)
                return null;

            foreach (var cqportal in db.cq_portal.Where(x => x.mapid == mapid))
            {
                if (cqportal.portal_idx == target.passway_mapportal)
                    portal = cqportal;
            }

            return portal;
        }
    }
}