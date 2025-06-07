using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Mana/magic power component for spellcasting entities. Contains current and maximum mana
/// values with change tracking for network synchronization. Used by ItemUseSystem for mana
/// restoration via consumable items (potions). Essential for magic-based gameplay mechanics,
/// spell casting costs, and mana regeneration. Currently uses simple change tracking rather
/// than auto-network synchronization.
/// </summary>
public struct ManaComponent(ushort mana, ushort maxMana)
{
    public long ChangedTick = NttWorld.Tick;
    public ushort Mana = mana;
    public ushort MaxMana = maxMana;
}