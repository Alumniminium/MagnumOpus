using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DirectionSystem : PixelSystem<DirectionComponent>
    {
        public DirectionSystem() : base("Direction System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref DirectionComponent dir)
        {
            if (dir.ChangedTick != PixelWorld.Tick)
                return;

            var msg = MsgAction.Create(ntt.NetId, 0, 0, 0, dir.Direction, MsgActionType.ChangeFacing);
            // ntt.NetSync(ref msg, true);
        }
    }
}