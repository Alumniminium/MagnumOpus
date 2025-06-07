using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Money reward component that marks entities as containing pickupable money. Contains the
/// amount of gold to be awarded when picked up. Used by PickupSystem to distinguish money
/// entities from item entities during pickup processing. When collected, the money amount
/// is added to the player's inventory and special pickup animations are triggered for
/// large amounts. Essential for money drop and pickup mechanics.
/// </summary>
public struct MoneyRewardComponent(int amount)
{
    public int Amount = amount;
}