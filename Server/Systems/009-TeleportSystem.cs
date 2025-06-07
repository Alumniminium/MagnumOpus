using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles entity teleportation between locations and maps.
    /// Manages position updates, spatial hash transfers, and client synchronization for seamless teleportation.
    /// </summary>
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent, ViewportComponent>
    {
        /// <summary>
        /// Initializes the TeleportSystem with limited threading and debug logging enabled.
        /// </summary>
        public TeleportSystem() : base("Teleport", threads: 1 / 4, log: false) { }

        /// <summary>
        /// Processes an entity's teleportation request, updating position and notifying clients.
        /// </summary>
        /// <param name="ntt">The entity being teleported</param>
        /// <param name="tpc">Teleport component containing destination coordinates and map</param>
        /// <param name="pos">Entity's position component to update</param>
        /// <param name="vwp">Viewport component for visibility management</param>
        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, new Vector2(tpc.X, tpc.Y), pos.Map, tpc.Map, SpacialHashUpdatType.Move);
            var viewportUpdate = new ViewportUpdateTagComponent();
            ntt.Set(ref viewportUpdate, ref spatialUpdate);

            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;

            ntt.Set<ViewportUpdateTagComponent>();

            var despawnPacket = MsgAction.Create(ntt.Id, ntt.Id, 0, 0, 0, Enums.MsgActionType.RemoveEntity);
            ntt.NetSync(ref despawnPacket, true, true);
            var teleportPacket = MsgAction.Create(ntt.Id, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref teleportPacket);
            var mapStatusPacket = MsgMapStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(ref mapStatusPacket);

            ntt.Remove<TeleportComponent>();

            if (IsLogging)
                FConsole.WriteLine("[{0}] Teleported '{1}' to {2}, {3}, {4}", NttWorld.Tick, ntt.Id, tpc.Map, tpc.X, tpc.Y);
        }
    }
}