// -------- Yi --------
// Project: Library File: MsgActionType.cs 
// Created: 27/10/2015/2015 at 3:09 PM
// Last Edit: 08/12/2015 at 12:31 PM
// By: Buddha

namespace MagnumOpus.Enums
{
    public enum MsgActionType : short
    {
        None = 0,
        SendLocation = 74,
        SendItems = 75,
        SendAssociates = 76,
        SendProficiencies = 77,
        SendSpells = 78,
        ChangeFacing = 79,
        ChangeAction = 81,
        EnterPortalChangeMap = 85,
        Teleport = 86,
        LevelUp = 92,
        XpClear = 93,
        Revive = 94,
        DelRole = 95,
        SetKillMode = 96,
        ConfirmGuild = 97,
        Mine = 99,
        /// <summary>
        /// [101]
        /// Data2 = TeamMemberId,
        /// Data3Low = PositionX,
        /// Data3High = PositionY
        /// </summary>
        TeamMemberPos = 101,
        QueryEntity = 102,
        AbortMagic = 103,
        MapARGB = 104,
        MapStatus = 105,
        /// <summary>
        /// [106]
        /// Data3Low = PositionX,
        /// Data3High = PositionY
        /// </summary>
        QueryTeamMember = 106,
        Kickback = 108,
        DropMagic = 109,
        DropSkill = 110,
        /// <summary>
        /// [111]
        /// Data2 = BoothId,
        /// Data3Low = PositionX,
        /// Data3High = PositionY,
        /// Data4 = Direction
        /// </summary>
        CreateBooth = 111,
        SuspendBooth = 112,
        ResumeBooth = 113,
        LeaveBooth = 114,
        PostCommand = 116,
        /// <summary>
        /// [117]
        /// Data2 = TargetId
        /// </summary>
        QueryEquipment = 117,
        AbortTransform = 118,
        EndFly = 120,
        /// <summary>
        /// [121]
        /// Data2
        /// </summary>
        GetMoney = 121,
        QueryEnemy = 123,
        OpenDialog = 126,
        GuardJump = 130,
        Jump = 133,
        /// <summary>
        /// [134] 
        /// Data1 = EntityId,
        /// Data3Low = PositionX,
        /// Data3High = PositionY
        /// </summary>
        SpawnEffect = 134,
        /// <summary>
        /// [135] 
        /// Data1 = EntityId
        /// </summary>
        RemoveEntity = 135,
        TeleportReply = 138,
        ChangeFace = 142,
        DeathConfirmation = 145,
        /// <summary>
        /// [148]
        /// Data1 = FriendId
        /// </summary>
        QueryAssociateInfo = 148,
        // ChangeFace = 151,
        ItemsDetained = 155,
        NinjaStep = 156,
        HideInterface = 158,
        OpenUpgrade = 160,
        /// <summary>
        /// [161] 
        /// Data1 = Mode (0=none,1=away)
        /// </summary>
        AwayFromKeyboard = 161,
        PathFinding = 162,
        DragonBallDropped = 165,
        TableState = 233,
        TablePot = 234,
        TablePlayerCount = 235,
        /// <summary>
        /// [310]
        /// Data2 = FriendId
        /// </summary>
        QueryFriendEquip = 310,
        QueryStatInfo = 408,
    }
}