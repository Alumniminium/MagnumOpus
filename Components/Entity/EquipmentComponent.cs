using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct EquipmentComponent
    {
        public readonly Dictionary<MsgItemPosition, NTT> Items;

        [JsonConstructor]
        public EquipmentComponent(Dictionary<MsgItemPosition, NTT>? items) => Items = items ?? new()
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
}