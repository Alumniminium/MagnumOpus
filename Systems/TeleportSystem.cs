using System.Numerics;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent>
    {
        public TeleportSystem() : base("Teleport", threads: 2) { }

        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos)
        {
            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;
            pos.ChangedTick = NttWorld.Tick;

            var tpP = MsgAction.Create(ntt.Id, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref tpP);
            var mapStatus = MsgMapStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(ref mapStatus);

            ntt.Remove<TeleportComponent>();
            if (IsLogging)
                Logger.Debug("[{tick}] Teleported '{0}' to {1}, {2}, {3}", NttWorld.Tick, Name, ntt, tpc.Map, tpc.X, tpc.Y);
        }
    }
}