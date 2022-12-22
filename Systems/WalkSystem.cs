using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class WalkSystem : PixelSystem<PositionComponent, WalkComponent, DirectionComponent>
    {
        public WalkSystem() : base("Walk System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref WalkComponent wlk, ref DirectionComponent dir)
        {
            var deltaX = Constants.DeltaX[(int)wlk.Direction];
            var deltaY = Constants.DeltaY[(int)wlk.Direction];
            pos.Position.X += deltaX;
            pos.Position.Y += deltaY;
            
            pos.ChangedTick = PixelWorld.Tick;
            dir.Direction = wlk.Direction;
            ntt.NetSync(MsgWalk.Create(ntt.NetId, wlk.Direction, wlk.IsRunning), true);
            FConsole.WriteLine($"[{nameof(WalkSystem)}] {ntt.NetId} -> {wlk.Direction} -> {pos.Position}");
            ntt.Remove<WalkComponent>();
        }
    }
}