using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class EmoteSystem : PixelSystem<EmoteComponent, DirectionComponent>
    {
        public EmoteSystem() : base("Emote System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref EmoteComponent emo, ref DirectionComponent dir)
        {
            if (emo.ChangedTick != PixelWorld.Tick)
                return;

            var msg = MsgAction.Create(ntt.NetId, (int)emo.Emote, 0,0, dir.Direction, MsgActionType.ChangeAction);
            ntt.NetSync(ref msg, true);
        }
    }
}