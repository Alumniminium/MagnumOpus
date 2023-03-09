using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestChangeEquipComponent
    {
        public readonly int EntityId;
        public readonly int ItemNetId;
        public readonly MsgItemPosition Slot;
        public readonly bool Equip;

        public RequestChangeEquipComponent(int entityId, int itemNetId, int slot, bool equip)
        {
            EntityId = entityId;
            ItemNetId = itemNetId;
            Slot = (MsgItemPosition)slot;
            Equip = equip;
        }

        public override int GetHashCode() => EntityId;
    }
}