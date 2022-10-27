using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Simulation.Components;

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

            pos.Position.X += (ushort)Constants.DeltaX[(sbyte)dir.Direction];
            pos.Position.Y += (ushort)Constants.DeltaY[(sbyte)dir.Direction];
        }
    }
}