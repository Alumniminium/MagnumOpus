using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct EquipmentComponent
    {
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

        public EquipmentComponent(Dictionary<MsgItemPosition, NTT> items) => Items = items;
    }
}