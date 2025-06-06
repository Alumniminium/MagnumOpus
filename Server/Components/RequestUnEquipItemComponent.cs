using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct RequestChangeEquipComponent(int itemNetId, int slot, bool equip)
    {
        public int ItemNetId = itemNetId;
        public MsgItemPosition Slot = (MsgItemPosition)slot;
        public bool Equip = equip;
    }
}