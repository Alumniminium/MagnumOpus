namespace MagnumOpus.Enums;

public enum MsgActionType : short
{
    None = 0,
    /// <summary>
    /// Client sends this to ask which map to load (on login and when teleporting)
    /// Server fills mapId, X and Y and sends it back to the client
    /// </summary>
    QueryLocation = 74,

    /// <summary>
    /// Client sends this to the server when it wants to populate the client's inventory
    /// The server will send the items to the client and then echo back the packet with the same data
    /// </summary>
    QueryItems = 75,

    /// <summary>
    /// Client sends this to the server when it wants to populate the client's friend list
    /// The server will send the list to the client and then echo back the packet with the same data
    /// </summary>
    QueryFriends = 76,

    /// <summary>
    /// Client sends this to the server when it wants to populate the client's proficiencies
    /// The server will send the list to the client and then echo back the packet with the same data
    /// </summary>
    QuerySkills = 77,

    /// <summary>
    /// Client sends this to the server when it wants to populate the client's spellbook
    /// The server will send the list to the client and then echo back the packet with the same data
    /// </summary>
    QuerySpells = 78,

    /// <summary>
    /// Client sends this to the server when the player holds shift and clicks around causing the character to change facing
    /// The server will update the client's facing and echo back the packet without changing anything
    /// The packet is also broadcast to all players in visible range
    /// </summary>
    UpdateFacing = 79,

    /// <summary>
    /// Client sends this to the server when the player clicks on an emote button
    /// The server will update the emote of the client and then echo back the packet with the same data 
    /// The packet is also broadcast to all players in visible range
    /// </summary>
    UpdateEmote = 81,

    /// <summary>
    /// Client sends this when jumping into a portal on the map
    /// Surprisingly, you can reply with a MsgTransfer and make the client
    /// connect to a different server and restart the login sequence.
    /// </summary>
    EnterPortalChangeMap = 85,

    /// <summary>
    /// Unsure
    /// </summary>
    Teleport = 86,

    /// <summary>
    /// Sending this to the client with a given uniqueId will cause the client to show the level up animation on that entity
    /// I'm not sure if it has other effects
    /// </summary>
    UpdateLevelUp = 92,

    /// <summary>
    /// Clears the player's XP Skill Bar.
    /// The server must keep track when the XP bar was Full and the XP Skill Bar triggered to clear it when the bar runs out on the client.
    /// </summary>
    XpClear = 93,

    /// <summary>
    /// Client sends this to the server when the Respawn button is clicked
    /// We echo back the packet to remove the revive buttons on the client
    /// </summary>
    Revive = 94,

    /// <summary>
    /// Client sends this to the server when the player clicks on the Delete Character button
    /// This is a vere nieche feature, so we echo back the packet and disconnect the client
    /// </summary>
    DelRole = 95,

    /// <summary>
    /// Client sends this to the server when the player clicks on the PK Button
    /// We need to echo back the packet or the client's UI will not update.
    /// I think this even Updates the client's UI when the packet comes without being requested first.
    /// </summary>
    SetKillMode = 96,

    /// <summary>
    /// Client sends this during login to request the guild the player is in
    /// Server sends back the guild name and the guild leader's name if any
    /// then echoes back the packet as it came in
    /// </summary>
    QueryGuild = 97,

    /// <summary>
    /// Client sends this when right-clicking on a mine map
    /// The MapFlags must be MAPTYPE_MINEFIELD or no packet will be sent
    /// </summary>
    Mine = 99,

    /// <summary>
    /// [101]
    /// Data2 = TeamMemberId,
    /// Data3Low = PositionX,
    /// Data3High = PositionY
    /// </summary>
    QueryTeamLeaderPos = 101,

    /// <summary>
    /// Client sends this to the server when it wants a new SpawnPacket for an entity
    /// This shouldn't happen often with correct viewport management, but it appears
    /// that the client itself has a race-condition that causes this to happen
    /// Or I have Skill Issues
    /// </summary>
    QueryEntity = 102,
    AbortMagic = 103,
    /// <summary>
    /// Lets us put a tint over the viewport of the client
    /// This does not apply to UI elements, only the World
    /// </summary>
    MapARGB = 104,

    /// <summary>
    /// Unknown
    /// </summary>
    MapStatus = 105,

    /// <summary>
    /// When a client overs a teammate's head, the server will send this packet to the client
    /// When we respond correctly, a blue star will appear on the clients minimap indicating where
    /// the teammate is located
    /// 
    /// Data3Low = PositionX,
    /// Data3High = PositionY
    /// </summary>
    QueryTeamMember = 106,
    Kickback = 108,

    /// <summary>
    /// Server sends this to remove a spell from the client's spellbook
    /// This is used to replace a low level spell with a higher level one
    /// Or to remove a spell that is no longer needed
    /// </summary>
    UnlearnSpell = 109,

    /// <summary>
    /// Server sends this to remove a skill from the client's skill book
    /// This is used to replace a low level skill with a higher level one
    /// Or to remove a skill that is no longer needed
    /// </summary>
    UnlearnSkill = 110,

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
    /// <summary>
    /// This is some sort of RPC that lets the server open UI windows on the client
    /// The effects can be severe and crash the client
    /// Very exciting to fuzz around with
    /// </summary>
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
    /// <summary>
    /// Sending this makes entities jump somewhere.
    /// This works on: Players, Monsters, NPCs (all types, but they don't have animations)
    /// This does not work on: Items, Traps
    /// </summary>
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
    RemoveEntity = 132,
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