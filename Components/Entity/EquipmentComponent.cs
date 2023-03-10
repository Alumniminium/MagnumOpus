using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public  struct EquipmentComponent
    {
        public  Dictionary<MsgItemPosition, NTT> Items;

        [JsonIgnore] public  ref NTT Head => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Head, out _);
        [JsonIgnore] public  ref NTT Necklace => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Necklace, out _);
        [JsonIgnore] public  ref NTT Garment => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Garment, out _);
        [JsonIgnore] public  ref NTT Bottle => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Bottle, out _);
        [JsonIgnore] public  ref NTT ARmor => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Armor, out _);
        [JsonIgnore] public  ref NTT Ring => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Ring, out _);
        [JsonIgnore] public  ref NTT LeftWeapon => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.LeftWeapon, out _);
        [JsonIgnore] public  ref NTT RightWeapon => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.RightWeapon, out _);
        [JsonIgnore] public  ref NTT Boots => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Boots, out _);

        public EquipmentComponent() => Items = new()
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