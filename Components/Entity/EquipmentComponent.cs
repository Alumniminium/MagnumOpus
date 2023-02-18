using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public NTT Head => Items[MsgItemPosition.Armor];
        public NTT Necklace => Items[MsgItemPosition.Necklace];
        public NTT Garment => Items[MsgItemPosition.Garment];
        public NTT Bottle => Items[MsgItemPosition.Bottle];
        public NTT Armor => Items[MsgItemPosition.Armor];
        public NTT Ring => Items[MsgItemPosition.Ring];
        public NTT MainHand => Items[MsgItemPosition.RightWeapon];
        public NTT OffHand => Items[MsgItemPosition.LeftWeapon];
        public NTT Boots => Items[MsgItemPosition.Boots];

        public Dictionary<MsgItemPosition, NTT> Items;

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Items = new Dictionary<MsgItemPosition, NTT>
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