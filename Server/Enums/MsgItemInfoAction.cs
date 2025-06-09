namespace MagnumOpus.Enums;

public enum MsgItemInfoAction : byte
{
    None = 0,

    /// <summary>
    /// Adds the item to the player
    /// Used together with MsgItemPosition to indicate where to add the item to.
    /// Examples: Inventory or Equipement Slot
    /// </summary>
    AddItem = 1,
    /// <summary>
    /// Adds the item to the current trade window
    /// </summary>
    Trade = 2,

    /// <summary>
    /// Not sure what this does
    /// </summary>
    Update = 3,

    /// <summary>
    /// Adds the item to the other player's equipment
    /// Used together with MsgItemPosition
    /// </summary>
    OtherPlayerEquipement = 4,
}