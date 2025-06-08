namespace MagnumOpus.Enums;

public enum AssociateMode : byte
{
    // Unknown, maybe requests the list of friends?
    RequestFriend = 10,

    // Unknown 
    NewFriend = 11,

    /// <summary>
    /// C->S: No
    /// S->C: Server sends this to the client when a friend of theirs comes online
    /// </summary>
    SetOnlineFriend = 12,

    // We send this to the client when a friend of theirs goes offline
    /// </summary>
    /// C-S: No
    /// S->C: Server sends this to the client when a friend of theirs goes offline
    /// </summary>
    SetOfflineFriend = 13,

    /// <summary>
    /// C->S: When a player requests to remove a friend
    /// S->C: Server sends this to both players when one of them requests to remove the other
    /// </summary>
    RemoveFriend = 14,

    /// <summary>
    /// C->S: When a player requests to add a friend
    /// S->C: Server sends this to the players when they agree to become friends
    /// </summary>
    AddFriend = 15,

    /// <summary>
    /// C->S: No
    /// S->C: We send this to the client when an enemy of theirs comes online
    /// </summary>
    SetOnlineEnemy = 16,

    /// </summary>
    /// C->S: No
    /// S->C: We send this to the client when an enemy of theirs goes offline
    /// </summary>
    SetOfflineEnemy = 17,

    /// <summary>
    /// C->S: Client requests to remove an enemy
    /// S->C: Server sends this to the client to remove an enemy from the clients list
    /// </summary>
    RemoveEnemy = 18,

    /// <summary>
    /// C->S: Client requests to add an enemy
    /// S->C: Server sends this to the client to add an enemy to the clients list
    /// When a player is killed by another player, this is sent to the client to add the killer to the enemy list
    /// I think there is also an NPC to delcare an enemy.
    /// </summary>
    AddEnemy = 19
}