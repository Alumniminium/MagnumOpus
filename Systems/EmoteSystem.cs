using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class EmoteSystem : NttSystem<EmoteComponent, PositionComponent>
    {
        public EmoteSystem(bool log = false) : base("Emote", threads: 2, log) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in NTT ntt, ref EmoteComponent emo, ref PositionComponent pos)
        {
            if (emo.ChangedTick != NttWorld.Tick)
                return;

            if (IsLogging)
                Logger.Debug("{ntt} emote {emote}", ntt, emo.Emote);

            if (emo.Emote == Emote.Sit)
            {
                var stamina = MsgUserAttrib.Create(ntt.Id, 150, MsgUserAttribType.Stamina);
                ntt.NetSync(ref stamina);
            }

            var msg = MsgAction.Create(ntt.Id, (int)emo.Emote, 0, 0, pos.Direction, MsgActionType.ChangeAction);
            ntt.NetSync(ref msg, true);
        }
    }
}