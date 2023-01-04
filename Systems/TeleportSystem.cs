using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class TeleportSystem : PixelSystem<TeleportComponent, PositionComponent>
    {
        public TeleportSystem() : base("Teleport System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref TeleportComponent tpc, ref PositionComponent pos)
        {
            pos.Position = new Vector2(tpc.X, tpc.Y);
            pos.Map = tpc.Map;
            pos.ChangedTick = PixelWorld.Tick;

            FConsole.WriteLine($"[{nameof(TeleportSystem)}]: Teleported {ntt.NetId} to {tpc.Map} at {tpc.X}, {tpc.Y}");

            var tpP = MsgAction.Create(ntt.NetId, tpc.Map, tpc.X, tpc.Y, Enums.Direction.South, Enums.MsgActionType.SendLocation);
            ntt.NetSync(ref tpP);
            var mapStatus = MsgStatus.Create(tpc.Map, (uint)Enums.MapFlags.None);
            ntt.NetSync(in mapStatus);

            ntt.Remove<TeleportComponent>();
        }
    }
}