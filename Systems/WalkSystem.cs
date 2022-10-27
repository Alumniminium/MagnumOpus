using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class WalkSystem : PixelSystem<PositionComponent, WalkComponent, DirectionComponent>
    {
        public WalkSystem() : base("Walk System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && ntt.Type != EntityType.Npc && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref WalkComponent wlk, ref DirectionComponent dir)
        {
            if (wlk.ChangedTick == PixelWorld.Tick)
                dir.Direction = wlk.Direction;
            else
            {
                ntt.Remove<WalkComponent>();
                return;
            }

            pos.Position.X += Constants.DeltaX[(int)dir.Direction];
            pos.Position.Y += Constants.DeltaY[(int)dir.Direction];
            pos.ChangedTick = PixelWorld.Tick;

            FConsole.WriteLine($"[{nameof(WalkSystem)}] {ntt.Id} -> {dir.Direction} | {pos.Position}");
        }
    }
}