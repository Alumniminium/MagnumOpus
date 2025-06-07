using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
public struct RequestChangeEquipComponent(int itemNetId, int slot, bool equip)
{
    public int ItemNetId = itemNetId;
    public MsgItemPosition Slot = (MsgItemPosition)slot;
    public bool Equip = equip;
}