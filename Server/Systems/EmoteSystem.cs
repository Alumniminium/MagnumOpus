using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles entity emotes and related behaviors like stamina regeneration when sitting.
    /// Broadcasts emote changes to nearby players for visual synchronization.
    /// </summary>
    /// <param name="log">Enable debug logging for emote changes</param>
    public sealed class EmoteSystem(bool log = false) : NttSystem<EmoteComponent, PositionComponent>("Emote", threads: 1, log)
    {
        /// <summary>
        /// Filters out item entities from emote processing since they cannot emote.
        /// </summary>
        /// <param name="ntt">Entity to check for emote processing eligibility</param>
        /// <returns>True if entity can emote (not an item)</returns>
        protected override bool MatchesFilter(in NTT ntt) => !ntt.IsItem() && base.MatchesFilter(in ntt);

        /// <summary>
        /// Processes entity emotes, handles stamina regeneration for sitting, and broadcasts emote changes.
        /// </summary>
        /// <param name="ntt">The entity performing the emote</param>
        /// <param name="emo">Emote component containing the current emote state</param>
        /// <param name="pos">Position component for emote broadcasting</param>
        public override void Update(in NTT ntt, ref EmoteComponent emo, ref PositionComponent pos)
        {
            if (emo.Emote == Emote.Sit && ntt.Has<StaminaComponent>())
            {
                ref var stamina = ref ntt.Get<StaminaComponent>();

                if (stamina.ChangedTick + (NttWorld.TargetTps / 3) < NttWorld.Tick && stamina.Stamina < stamina.MaxStamina)
                    stamina.Stamina = (byte)Math.Clamp(stamina.Stamina + 5, 0, stamina.MaxStamina);
            }

            if (emo.ChangedTick != NttWorld.Tick)
                return;

            if (IsLogging)
                FConsole.WriteLine("{ntt} emote {emote}", ntt, emo.Emote);

            var actionMessage = MsgAction.Create(ntt.Id, (int)emo.Emote, 0, 0, pos.Direction, MsgActionType.ChangeAction);
            ntt.NetSync(ref actionMessage, broadcast: true);
        }
    }
}