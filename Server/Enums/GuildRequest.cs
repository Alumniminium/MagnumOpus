namespace MagnumOpus.Enums;

public enum GuildRequest
{
    None = 0,
    ApplyJoin = 1, // Apply to join guild/syndicate, id (申请加入黑社会, id)
    InviteJoin = 2, // Invite to join guild/syndicate, id (邀请加入黑社会, id)
    LeaveGuild = 3, // Leave guild/syndicate (脱离黑社会)
    KickOutMember = 4, // Kick out guild member, name (开除黑社会成员, name)
    QueryGuildName = 6, // Query guild name (查询帮派名字)
    SetAlly = 7, // Form alliance (结盟)				// to client, npc(npc_id is syn_id, same follow)
    ClearAlly = 8, // Dissolve alliance (解除结盟)			// to client, npc
    SetEnemy = 9, // Set as enemy (树敌)				// to client, npc
    RemoveEnemy = 10, // Clear enemy status (解除树敌)			// to client, npc
    DonateMoney = 11, // Guild members donate money (帮众捐钱)
    QueryGuildInfo = 12, // Query guild attributes/info (查询帮派信息)		// to server
    SetGuildId = 14, // Add guild ID (添加帮派ID)		// to client
    MergeSubGuild = 15, // Merge sub-guild (合并堂口) // to client // dwData is merged, target is master
    MergeGuild = 16, // Merge guild (合并帮派) // to client // dwData is merged, target is master
    SetWhiteGuild = 17, // White guild ID (白帮帮派ID) // Send ID_NONE if not occupied
    SetBlackGuild = 18, // Black guild ID (黑帮帮派ID) // Send ID_NONE if not occupied
    DestroyGuild = 19, // World broadcast, destroy guild (世界广播，删除帮派)
    SetMantle = 20, // World broadcast, mantle/cape (世界广播，披风) // add huang 2004.1.1       

    //_APPLY_ALLY = 21,			// Apply for alliance (申请结盟)			// to server&client, idTarget=SynLeaderID
    //_CLEAR_ALLY = 22,			// Clear alliance (清除结盟)			// to server

    //_SET_ANTAGONIZE = 23,			// Set antagonize (树敌) client to server
    //_CLEAR_ANTAGONIZE = 24,			// Clear antagonize (解除树敌) client to server

    //NPCMSG_CREATE_SYN = 101,			// Notify NPC server add guild (通知NPC服务器添加帮派)	// to npc
    //NPCMSG_DESTROY_SYN = 102,			// Notify NPC server destroy guild (通知NPC服务器删除帮派)	// to npc
    //KICKOUT_MEMBER_INFO_QUERY = 110,	// Guild leader queries kick request (帮主查询申请开除的成员)
    //KICKOUT_MEMBER_AGREE = 111,	// Guild leader agrees to kick member (帮主同意开除会员)
    //KICKOUT_MEMBER_NOAGREE = 112,	// Guild leader disagrees to kick member (帮主不同意开除会员)
    //SYNMEMBER_ASSIGN = 113,			// Guild member assignment (帮派成员编制)	
    //SYN_CHANGE_NAME = 114,			// Guild rename (帮派改名)
    //SYN_CHANGE_SUBNAME = 114,		// Sub-group rename (分团改名)
    //SYN_CHANGE_SUBSUBNAME = 115,		// Sub-team rename (分队改名)
    //SYN_DEMISE = 116,		// Demise/transfer leadership (禅让)
    //SYN_SET_ASSISTANT = 117,		// Set assistant leader (设置副帮主)
    //SYN_SET_TEAMLEADER = 118,		// Set guild team leader (设置帮派队长)
    //SYN_SET_PUBLISHTIME = 119,		// Set announcement time (设置公告时间)
}