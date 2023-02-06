using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class EmoteSystem : PixelSystem<EmoteComponent, BodyComponent>
    {
        public EmoteSystem() : base("Emote System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref EmoteComponent emo, ref BodyComponent bdt)
        {
            if (emo.ChangedTick != PixelWorld.Tick)
                return;

            if(emo.Emote == Emote.Sit)
            {
                var stamina = MsgUserAttrib.Create(ntt.NetId, 100, MsgUserAttribType.Stamina);
                ntt.NetSync(ref stamina);
            }

            var msg = MsgAction.Create(ntt.NetId, (int)emo.Emote, 0,0, bdt.Direction, MsgActionType.ChangeAction);
            ntt.NetSync(ref msg, true);
        }
    }
}