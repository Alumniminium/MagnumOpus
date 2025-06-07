// -------- Yi --------
// Project: Library File: AssociateMode.cs 
// Created: 27/10/2015/2015 at 3:09 PM
// Last Edit: 08/12/2015 at 12:31 PM
// By: Buddha

namespace MagnumOpus.Enums
{
    public enum AssociateMode : byte
    {
        // Client sends this to us when they click a player with the "Add Friend" cursor
        RequestFriend = 10,
        // Unknown
        NewFriend = 11,
        // We send this to the client when a friend of theirs comes online
        SetOnlineFriend = 12,
        // We send this to the client when a friend of theirs goes offline
        SetOfflineFriend = 13,
        // Client sends this to us when they click a player with the "Remove Friend" cursor
        RemoveFriend = 14,
        // Server sends this to the two players when they agree to become friends  
        AddFriend = 15,
        // We send this to the client when an enemy of theirs comes online
        SetOnlineEnemy = 16,
        // We send this to the client when an enemy of theirs goes offline
        SetOfflineEnemy = 17,

        RemoveEnemy = 18,
        AddEnemy = 19
    }
}