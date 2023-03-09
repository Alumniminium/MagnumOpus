using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestChangeEquipComponent
    {
        public readonly int ItemNetId;
        public readonly MsgItemPosition Slot;
        public readonly bool Equip;

        public RequestChangeEquipComponent(int itemNetId, int slot, bool equip)
        {
            ItemNetId = itemNetId;
            Slot = (MsgItemPosition)slot;
            Equip = equip;
        }

        
    }
}