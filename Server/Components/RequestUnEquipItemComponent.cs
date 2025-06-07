using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient equipment change request component that handles equipping and unequipping items.
/// Contains item network ID, equipment slot position, and equip/unequip flag. Not saved to
/// database (no SaveEnabled). Processed by EquipSystem to validate item ownership, manage
/// inventory-equipment transfers, update character appearance, handle stat calculations,
/// and synchronize equipment changes to clients. Essential for equipment management.
/// </summary>
public struct RequestChangeEquipComponent(int itemNetId, int slot, bool equip)
{
    public int ItemNetId = itemNetId;
    public MsgItemPosition Slot = (MsgItemPosition)slot;
    public bool Equip = equip;
}