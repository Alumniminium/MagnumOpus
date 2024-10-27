using HerstLib.IO;
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
            if (emo.Emote == Emote.Sit && ntt.Has<StaminaComponent>())
            {
                ref var sta = ref ntt.Get<StaminaComponent>();

                if (sta.ChangedTick + (NttWorld.TargetTps / 3) < NttWorld.Tick && sta.Stamina < sta.MaxStamina)
                    sta.Stamina = (byte)Math.Clamp(sta.Stamina + 5, 0, sta.MaxStamina);
            }

            if (emo.ChangedTick != NttWorld.Tick)
                return;

            if (IsLogging)
                FConsole.WriteLine("{ntt} emote {emote}", ntt, emo.Emote);

            var msg = MsgAction.Create(ntt.Id, (int)emo.Emote, 0, 0, pos.Direction, MsgActionType.ChangeAction);
            ntt.NetSync(ref msg, broadcast: true);
        }
    }
}