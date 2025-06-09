namespace MagnumOpus.Enums;

public enum MsgTeamAction
{
    /// <summary>
    /// C->S: When a player clicks the "Create" button in the Team UI
    /// S->C: Server sends this to the client to confirm the creation of the team
    /// </summary>
    Create = 0,

    /// <summary>
    /// C->S: When a player clicks the "Request Join" button in the Team UI
    /// S->C: No
    /// </summary>
    RequestJoin = 1,

    /// <summary>
    /// C->S: When a player clicks the "Leave Team" button in the Team UI
    /// S->C: Server sends this to the client to confirm the leaving of the team
    /// </summary>
    LeaveTeam = 2,

    /// <summary>
    /// C->S: When a player clicks the "Accept Invite" button in the Team UI
    /// S->C: No, server sends ...?
    /// </summary>
    AcceptInvite = 3,

    /// <summary>
    /// C->S: When a player clicks the "Invite" button in the Team UI and the target player
    /// S->C: No, server sends RequestJoin to the requested player
    /// </summary>
    Invite = 4,

    /// <summary>
    /// C->S: When a player clicks 
    /// S->C: ...?  
    /// </summary>
    AcceptJoin = 5,

    /// <summary>
    /// C->S: When a player clicks the "Dismiss" button in the Team UI
    /// S->C: Echo to everyone in the team?
    /// </summary>
    Dismiss = 6,
    /// <summary>
    /// C->S: When the Team Leader clicks the "Kick" button in the Team UI
    /// S->C: 
    /// </summary>
    Kick = 7,

    /// <summary>
    /// C->S: When a Team Leader clicks the "Forbid New Members" button in the Team UI
    /// S->C: Server sends this to the client to confirm
    /// </summary>
    ForbidNewMembers = 8,
    /// <summary>
    /// C->S: When a Team Leader clicks the "Forbid New Members" button in the Team UI
    /// S->C: Server sends this to the client to confirm
    /// </summary>
    AllowNewMembers = 9,

    /// <summary>
    /// C->S: When a Team Leader clicks the "Forbid Money" button in the Team UI 
    /// S->C: Upon joining the team, the server sends this to the client to show the checkbox checked
    /// </summary>
    ForbidMoney = 10,

    /// <summary>
    /// C->S: When a Team Leader clicks the "Allow Money" button in the Team UI 
    /// S->C: Upon joining the team, the server sends this to the client to show the checkbox checked
    /// </summary>
    AllowMoney = 11,

    /// <summary>
    /// C->S: When a Team Leader clicks the "Forbid Items" button in the Team UI 
    /// S->C: Upon joining the team, the server sends this to the client to show the checkbox checked
    /// </summary>
    ForbidItems = 12,

    /// <summary>
    /// C->S: When a Team Leader clicks the "Allow Items" button in the Team UI 
    /// S->C: Upon joining the team, the server sends this to the client to show the checkbox checked
    /// </summary>
    AllowItems = 13
}