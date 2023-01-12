using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public PixelEntity Head => Items[MsgItemPosition.Armor];
        public PixelEntity Necklace => Items[MsgItemPosition.Necklace];
        public PixelEntity Garment => Items[MsgItemPosition.Garment];
        public PixelEntity Bottle => Items[MsgItemPosition.Bottle];
        public PixelEntity Armor => Items[MsgItemPosition.Armor];
        public PixelEntity Ring => Items[MsgItemPosition.Ring];
        public PixelEntity MainHand => Items[MsgItemPosition.RightWeapon];
        public PixelEntity OffHand => Items[MsgItemPosition.LeftWeapon];
        public PixelEntity Boots => Items[MsgItemPosition.Boots];

        public Dictionary<MsgItemPosition, PixelEntity> Items;

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Items = new Dictionary<MsgItemPosition, PixelEntity>
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
        }

        public override int GetHashCode() => EntityId;
    }
}