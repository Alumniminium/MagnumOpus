using System.Runtime.InteropServices;
using MagnumOpus.Enums;
using Newtonsoft.Json;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character equipment component that manages all equipped items across different body slots.
/// Contains a dictionary mapping equipment positions (Head, Armor, Weapons, etc.) to item entities.
/// Provides direct property access to major equipment slots using CollectionsMarshal for performance.
/// Processed by EquipSystem for equipment changes, stat calculations, and appearance updates.
/// </summary>
public struct EquipmentComponent
{
    public Dictionary<MsgItemPosition, NTT> Items;

    [JsonIgnore] public readonly ref NTT Head => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Head, out _);
    [JsonIgnore] public readonly ref NTT Necklace => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Necklace, out _);
    [JsonIgnore] public readonly ref NTT Garment => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Garment, out _);
    [JsonIgnore] public readonly ref NTT Bottle => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Bottle, out _);
    [JsonIgnore] public readonly ref NTT Armor => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Armor, out _);
    [JsonIgnore] public readonly ref NTT Ring => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Ring, out _);
    [JsonIgnore] public readonly ref NTT LeftWeapon => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.SecondaryWeapon, out _);
    [JsonIgnore] public readonly ref NTT RightWeapon => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.PrimaryWeapon, out _);
    [JsonIgnore] public readonly ref NTT Boots => ref CollectionsMarshal.GetValueRefOrAddDefault(Items, MsgItemPosition.Boots, out _);

    [JsonConstructor]
    public EquipmentComponent() => Items = new()
    {
            { MsgItemPosition.Head, default },
            { MsgItemPosition.Necklace, default },
            { MsgItemPosition.Garment, default },
            { MsgItemPosition.Bottle, default },
            { MsgItemPosition.Armor, default },
            { MsgItemPosition.Ring, default },
            { MsgItemPosition.PrimaryWeapon, default },
            { MsgItemPosition.SecondaryWeapon, default },
            { MsgItemPosition.Boots, default }
    };
    public EquipmentComponent(Dictionary<MsgItemPosition, NTT> items) => Items = items;
}