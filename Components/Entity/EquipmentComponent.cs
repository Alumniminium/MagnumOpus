using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public readonly Dictionary<MsgItemPosition, NTT> Items = new()
        {
                { MsgItemPosition.Head, default },
                { MsgItemPosition.Necklace, default },
                { MsgItemPosition.Garment, default },
                { MsgItemPosition.Bottle, default },
                { MsgItemPosition.Armor, default },
                { MsgItemPosition.Ring, default },
                { MsgItemPosition.RightWeapon, default },
                { MsgItemPosition.LeftWeapon, default },
                { MsgItemPosition.Boots, default }
        };

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}