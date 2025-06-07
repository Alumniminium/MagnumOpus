using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent, ViewportComponent>
    {
        public TeleportSystem() : base("Teleport", threads: 1 / 4, log: false) { }

        // Handles entity teleportation between locations and maps. Manages position updates,
        // spatial hash transfers, and client synchronization for seamless teleportation. Sends
        // despawn packet to current location, updates entity position and map, then sends location
        // and map status packets to synchronize the client with the new environment.
        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            // === UPDATE SPATIAL SYSTEMS ===
            // Prepare spatial hash and viewport updates for position change
            var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, new Vector2(tpc.X, tpc.Y), pos.Map, tpc.Map, SpacialHashUpdatType.Move);
            var viewportUpdate = new ViewportUpdateTagComponent();
            ntt.Set(ref viewportUpdate, ref spatialUpdate);

            // === UPDATE ENTITY POSITION ===
            // Set new coordinates and map instantly
            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;

            // === CLIENT SYNCHRONIZATION ===
            // Send despawn packet to remove entity from old location
            var despawnPacket = MsgAction.Create(ntt.Id, ntt.Id, 0, 0, 0, Enums.MsgActionType.RemoveEntity);
            ntt.NetSync(ref despawnPacket, broadcast: true, ignoreSelf: false);

            // Send new location to client
            var teleportPacket = MsgAction.Create(ntt.Id, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref teleportPacket);

            // Update client map status
            var mapStatusPacket = MsgMapStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(ref mapStatusPacket);

            // === FINALIZE TELEPORT ===
            // Trigger additional viewport updates and cleanup
            ntt.Set<ViewportUpdateTagComponent>();
            ntt.Remove<TeleportComponent>();

            if (IsLogging)
                FConsole.WriteLine("{ntt} teleported to map {map} at {x}, {y}", ntt, tpc.Map, tpc.X, tpc.Y);
        }
    }
}