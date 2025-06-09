namespace MagnumOpus.Enums;

public enum MsgTextType : short
{
    /// <summary>
    /// This is the proximity chat channel, it is used for chatting with other players
    /// The message is sent to all players in the same map as the player
    /// </summary>
    Talk = 2000,

    /// <summary>
    /// This spawns a one-to-one chatbox with another player
    /// The other player must not really exist, this can be used to create a chatbot
    /// Whispers also create a small Icon on the screen so conversations can be tracked
    /// </summary>
    Whisper = 2001,

    /// <summary>
    /// Not sure what this is used for
    /// </summary>
    Action = 2002,
    /// <summary>
    /// Every team member gets a copy of the message, no matter where they are
    /// </summary>
    Team = 2003,

    /// <summary>
    /// Every guild member gets a copy of the message, no matter where they are
    /// </summary>
    Guild = 2004,

    /// <summary>
    /// This makes messages appear in the top left corner of the screen
    /// </summary>
    TopLeft = 2005,

    Spouse = 2006,
    Friend = 2009,
    Broadcast = 2010,

    /// <summary>
    /// This makes messages appear in the center of the screen
    /// </summary>
    Center = 2011,

    /// <summary>
    /// Only other ghosts and water taoists can see messages in this channel
    /// </summary>
    Ghost = 2013,

    Service = 2014,
    Dialog = 2100,

    /// <summary>
    /// This is used during the login sequence to trigger the client to:
    /// 1. Display the Character Creation screen (NEW_ROLE)
    /// 2. Load into the game world (ANSWER_OK)
    /// 3. Display error messages (Any other string)
    /// </summary>
    LoginInformation = 2101,

    /// <summary>
    /// This is used for shops to yell out a message every couple seconds
    /// This message does only appear as a chat bubble, not in the chat window
    /// </summary>
    VendorHawk = 2104,

    /// <summary>
    /// This forces the client to open the webpage in the default browser
    /// Except, its not neccessarily a webpage, it can be a filepath
    /// This can be used to execute code on the client, eg calling a javascript function
    /// </summary>
    Webpage = 2105,

    /// <summary>
    /// This is used clear the minimap and redraw the first line
    MiniMap = 2108,

    /// <summary>
    /// This is used to draw the next lines below the first line
    /// Some kind of partial update... weird stuff
    /// </summary>
    MiniMap2 = 2109,

    FriendsOfflineMessage = 2110,

    /// <summary>
    /// This is used to display the guild bulletin message in the guild UI
    /// </summary>
    GuildBulletin = 2111,

    /// <summary>
    /// This is used to display the trade tab board messages 
    /// </summary>
    TradeBoard = 2201,

    /// <summary>
    /// This is used to display the friend tab board messages 
    /// </summary>
    FriendBoard = 2202,

    /// <summary>
    /// This is used to display the team tab board messages 
    /// </summary>
    TeamBoard = 2203,

    /// <summary>
    /// This is used to display the guild tab board messages 
    /// </summary>
    GuildBoard = 2204,

    /// <summary>
    /// This is used to display the other tab board messages 
    /// </summary>
    OthersBoard = 2205
}