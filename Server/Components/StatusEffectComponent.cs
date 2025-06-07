using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Status effects component managing temporary character states and conditions. Contains status
/// effect flags (Dead, Frozen, Invisible, etc.) with automatic network synchronization for
/// real-time status updates. Used by ReviveSystem to clear death-related effects, DeathSystem
/// to apply death status, and TeamSystem for status verification. Critical for buff/debuff
/// systems, death states, and temporary character modifications.
/// </summary>
public struct StatusEffectComponent(in NTT entityId, StatusEffect effects = StatusEffect.None)
{
    public NTT NTT = entityId;
    public long ChangedTick = NttWorld.Tick;
    private StatusEffect _effects = effects;

    public StatusEffect Effects
    {
        readonly get => _effects;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _effects, value, MsgUserAttribType.StatusEffect, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}