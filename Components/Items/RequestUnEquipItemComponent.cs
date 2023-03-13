using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.Items
{
    [Component]
    public struct RequestChangeEquipComponent
    {
        public int ItemNetId;
        public MsgItemPosition Slot;
        public bool Equip;

        public RequestChangeEquipComponent(int itemNetId, int slot, bool equip)
        {
            ItemNetId = itemNetId;
            Slot = (MsgItemPosition)slot;
            Equip = equip;
        }
    }
}