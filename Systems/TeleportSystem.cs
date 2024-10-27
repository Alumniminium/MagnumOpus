using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent, ViewportComponent>
    {
        public TeleportSystem() : base("Teleport", threads: 1, log: true) { }

        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            var shc = new SpatialHashUpdateComponent(pos.Position, new Vector2(tpc.X, tpc.Y), pos.Map, tpc.Map, SpacialHashUpdatType.Move);
            var vpu = new ViewportUpdateTagComponent();
            ntt.Set(ref vpu, ref shc);

            pos.ChangedTick = NttWorld.Tick;
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
                FConsole.WriteLine("[{tick}] Teleported '{0}' to {1}, {2}, {3}", NttWorld.Tick, ntt, tpc.Map, tpc.X, tpc.Y);
        }
    }
}