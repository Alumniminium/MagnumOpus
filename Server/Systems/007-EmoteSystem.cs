using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class EmoteSystem(bool log = false) : NttSystem<EmoteComponent, PositionComponent>("Emote", threads: 1, log)
{
    protected override bool MatchesFilter(in NTT ntt) => !ntt.IsItem() && base.MatchesFilter(in ntt);

    // Handles entity emotes and related behaviors like stamina regeneration when sitting.
    // The system regenerates stamina for sitting entities (5 points every 1/3 second) and
    // broadcasts emote changes to nearby players for visual synchronization. Only processes
    // emotes that changed in the current tick to avoid unnecessary network traffic.
    public override void Update(in NTT ntt, ref EmoteComponent emo, ref PositionComponent pos)
    {
        // === HANDLE SITTING STAMINA REGENERATION ===
        // Regenerate stamina for sitting entities every 1/3 second
        if (emo.Emote == Emote.Sit && ntt.Has<StaminaComponent>())
        {
            ref var stamina = ref ntt.Get<StaminaComponent>();
            var regenInterval = stamina.ChangedTick + (NttWorld.TargetTps / 3);

            if (regenInterval < NttWorld.Tick && stamina.Stamina < stamina.MaxStamina)
                stamina.Stamina = (byte)Math.Clamp(stamina.Stamina + 5, 0, stamina.MaxStamina);
        }

        // Only process emotes that changed this tick to avoid spam
        if (emo.ChangedTick != NttWorld.Tick)
            return;

        // === BROADCAST EMOTE CHANGE ===
        // Send emote update to nearby players for visual synchronization
        if (IsLogging)
            FConsole.WriteLine("{ntt} changed emote to {emote}", ntt, emo.Emote);

        var msgAction = MsgAction.Create(ntt.Id, (int)emo.Emote, 0, 0, pos.Direction, MsgActionType.UpdateEmote);
        ntt.NetSync(ref msgAction, broadcast: true);
    }
}