using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Core health management component for all living entities. Contains current and maximum health
/// with automatic network synchronization for real-time health updates. Health changes trigger
/// network packets to clients. Used by DamageSystem for damage application, ReviveSystem for
/// health restoration, ItemUseSystem for healing consumables, and LevelingSystem for health
/// restoration on level up. Critical for combat, death, and healing mechanics.
/// </summary>
public struct HealthComponent(in NTT ntt, int health = 100, int maxHealth = 100)
{
    public long ChangedTick = NttWorld.Tick;
    public NTT NTT = ntt;
    private int _health = health;
    private int _maxHealth = maxHealth;

    public int Health
    {
        readonly get => _health;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _health, value, MsgUserAttribType.Health, NTT);
    }

    public int MaxHealth
    {
        readonly get => _maxHealth;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _maxHealth, value, MsgUserAttribType.MaxHealth, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}