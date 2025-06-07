using MagnumOpus.Enums;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Core item data component containing all item properties and enhancement information.
/// Stores item ID, durability (current/max), upgrades (Plus, Bless, Enchant), gems,
/// reborn effects, and stack amounts. Used by multiple systems for item operations:
/// EquipSystem for equipment handling, ShopSystem for transactions, ItemUseSystem for
/// consumables, DropItemSystem for ground items. Essential for all item-related gameplay.
/// </summary>
public struct ItemComponent
{
    public int Id;
    public ushort CurrentDurability;
    public ushort MaximumDurability;
    public byte Plus;
    public byte Bless;
    public byte Enchant;
    public byte Gem1;
    public byte Gem2;
    public RebornItemEffect RebornEffect;
    public int CustomTextId;
    public byte StackAmount
    {
        readonly get => (byte)(CustomTextId % 10000000);
        set
        {
            var amount = CustomTextId % 10000000;
            CustomTextId -= amount;
            amount = value;
            CustomTextId += amount;
        }
    }

    public ItemComponent(int itemId, ushort currentDurability, ushort maximumDurability, byte stackAmount, byte plus, byte bless, byte enchant, byte gem1, byte gem2, RebornItemEffect rebornEffect, int customTextId)
    {
        Id = itemId;
        CurrentDurability = currentDurability;
        MaximumDurability = maximumDurability;
        Plus = plus;
        Bless = bless;
        Enchant = enchant;
        Gem1 = gem1;
        Gem2 = gem2;
        RebornEffect = rebornEffect;
        CustomTextId = customTextId;
        StackAmount = stackAmount;
    }
}