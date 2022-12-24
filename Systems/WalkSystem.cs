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
            pos.ChangedTick = PixelWorld.Tick;
            pos.Position.X += deltaX;
            pos.Position.Y += deltaY;
            dir.Direction = wlk.Direction;

            Game.Grids[pos.Map].Move(in ntt, ref pos);
            
            var msg = MsgWalk.Create(ntt.NetId, wlk.Direction, wlk.IsRunning);
            ntt.NetSync(ref msg, true);
            ntt.Remove<WalkComponent>();

            // FConsole.WriteLine($"[{nameof(WalkSystem)}] {ntt.NetId} -> {wlk.Direction} -> {pos.Position}");
        }
    }
}