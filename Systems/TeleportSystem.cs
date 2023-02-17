using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class TeleportSystem : NttSystem<TeleportComponent, PositionComponent>
    {
        public TeleportSystem() : base("Teleport", threads: 2) { }

        public override void Update(in NTT ntt, ref TeleportComponent tpc, ref PositionComponent pos)
        {
            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;
            pos.ChangedTick = NttWorld.Tick;

            var tpP = MsgAction.Create(ntt.NetId, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref tpP);
            var mapStatus = MsgMapStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(ref mapStatus);

            ntt.Remove<TeleportComponent>();
            FConsole.WriteLine($"[{nameof(TeleportSystem)}]: Teleported {ntt.NetId} to {tpc.Map} at {tpc.X}, {tpc.Y}");
        }
    }
}