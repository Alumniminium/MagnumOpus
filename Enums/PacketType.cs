﻿// -------- Yi --------
// Project: Library File: PacketType.cs 
// Created: 27/10/2015/2015 at 3:09 PM
// Last Edit: 08/12/2015 at 12:31 PM
// By: Buddha

namespace MagnumOpus.Enums
{
    public enum PacketId : ushort
    {
        MsgRole = 1001,
        MsgText = 1004,
        MsgWalk = 1005,
        MsgHero = 1006,
        MsgItem = 1009,
        MsgTick = 1012,
        MsgAction = 1010,
        MsgSpwan = 1014,
        MsgName = 1015,
        MsgUpdate = 1017,
        MsgFriend = 1019,
        MsgInteract = 1022,
        MsgTeam = 1023,
        MsgSockEm = 1027,
        MsgForge = 1028,
        MsgTime = 1033,
        MsgConnect = 1051,
        MsgLogin = 1052,
        MsgTransfer = 1055,
        MsgTrade = 1056,
        MsgFloorItem = 1101,
        MsgStorage = 1102,
        MsgMagicEffect = 1105,
        MsgGuild = 1107,
        MsgNpc = 1109,
        MsgGuildInfo = 1112,
        MsgNpcSpawn = 2030,
        MsgDialog = 2031,
        MsgDialog2 = 2032,
        MsgCompose = 2036,
    }
}