using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent>
    {
        public TeleportSystem() : base("Teleport", threads: 1, log: true) { }

        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos)
        {
            var shc = new SpatialHashUpdateComponent(pos.Position, new Vector2(tpc.X, tpc.Y), pos.Map, tpc.Map, SpacialHashUpdatType.Move);
            ntt.Set(ref shc);

            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;

            ntt.Set<ViewportUpdateTagComponent>();

            var tpP = MsgAction.Create(ntt.Id, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref tpP);
            var mapStatus = MsgMapStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(ref mapStatus);

            ntt.Remove<TeleportComponent>();
            if (IsLogging)
                Logger.Debug("[{tick}] Teleported '{0}' to {1}, {2}, {3}", NttWorld.Tick, ntt, tpc.Map, tpc.X, tpc.Y);
        }
    }
}