# CQ_Action Documentation

*Original by paled, 2003.1.25*

## Parameter Formatting

- `%%` - Expression may demonstrate `%` mark
- `%nnXXX` - Expression demonstration width, all left aligned. Ultra long according to virtual length demonstration (example: id is 1234 NPC `%6id` demonstration is `1234`)

## System Parameters

| Parameter | Description |
|-----------|-------------|
| `date_stamp` | Uses in calculating only |
| `time` | Uses in calculating only |
| `accept` | Client side uploading substring (`%accept0` ~ `%accept3`) |

## Item Parameters

| Parameter | Description |
|-----------|-------------|
| `item_type` | Item type identifier |
| `item_data` | Item data field |

## NPC Parameters

| Parameter | Description |
|-----------|-------------|
| `datastr` | NPC data string |
| `data` | NPC data fields (`%data0` ~ `%data3`) |
| `name` | NPC name |
| `id` | NPC ID in database |
| `npc_x` | NPC X coordinate |
| `npc_y` | NPC Y coordinate |
| `npc_ownerid` | NPC owner ID |

## User Parameters

| Parameter | Description |
|-----------|-------------|
| `user_id` | User ID |
| `user_map_id` | User map ID |
| `user_map_x` | User map X coordinate |
| `user_map_y` | User map Y coordinate |
| `user_home_id` | User home ID |
| `syn_id` | Syndicate ID |
| `syn_name` | Syndicate name |
| `user_name` | User name |
| `mate_name` | Mate name |
| `map_owner_id` | Map owner ID |
| `map_owner_type` | Map owner type |
| `ally_syn` | Allied syndicate (`%ally_syn0` ~ `%ally_syn4`) |
| `enemy_syn` | Enemy syndicate (`%enemy_syn0` ~ `%enemy_syn4`) |
| `tutor_exp` | Teacher experiences |
| `student_exp` | Apprentice contributes experience |
| `exploit` | Meritorious service value |

### Faction Parameters

| Parameter | Description |
|-----------|-------------|
| `available_fund` | Extraction faction may assign the fund |

### User Iterator Parameters

| Parameter | Description |
|-----------|-------------|
| `iter_value` | Iterator value |
| `iter_syn_name` | Iterator syndicate name |
| `iter_syn_leader` | Iterator syndicate leader |
| `iter_syn_money` | Iterator syndicate money |
| `iter_syn_amount` | Iterator syndicate amount |
| `iter_syn_fealty` | Iterator syndicate fealty |
| `iter_member_name` | Returns plays family name (%iter is plays family ID) |
| `iter_member_rank` | Returns plays family rank title (%iter is plays family ID) |
| `iter_member_proffer` | Iterator member proffer |
| `iter_wanted` | Iterator wanted |
| `iter_police_wanted` | Iterator police wanted |
| `iter_upquality_gem` | Iterator up quality gem |
| `iter_uplevel_gem` | Iterator up level gem |
| `iter_cost_durrecover` | Iterator cost durability recover |
| `iter_game_card` | Iterator game card |
| `iter_game_card2` | Iterator game card 2 |
| `iter_table_datastr` | Iterator table data string |
| `iter_table_data` | Iterator table data (`%iter_table_data0` ~ `%iter_table_data3`) |
| `iter_item_data` | Iterator item data |

### Task System Iterator Parameters

| Parameter | Description |
|-----------|-------------|
| `iter_task_username` | Returns plays family's cq_user table name field |
| `iter_task_completenum` | Returns end of mission number of times |
| `iter_task_begintime` | Returns duty beginning time |

## Action Types

### System Actions (100-199)
*Need user ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_MENUTEXT` | 101 | Menu text | `data`: Number of lines (default 0), `param`: Text content |
| `ACTION_MENULINK` | 102 | Menu ultra link | `"text task_id align"` - align options: 0=left, 5=center, 9=right |
| `ACTION_MENUEDIT` | 103 | Menu input frame | `"len task_id text"` - len: input length, text: demonstration writing |
| `ACTION_MENUPIC` | 104 | Menu picture | `"x y pic_id task_id"` - task_id optional for clickable pictures |
| `ACTION_MENUBUTTON` | 110 | Menu button | Form with ultra link |
| `ACTION_MENULISTPART` | 111 | Menu tabulation item | `"task_id iter text..."` |
| `ACTION_MENUCREATE` | 120 | Menu foundation | `"cancel_task_id"` (optional) |
| `ACTION_RAND` | 121 | Examines probability | `"data1 data2"` - e.g., "10 100" = 1/10 chance is true |
| `ACTION_RANDACTION` | 122 | Random action | `"action0 action1... action7"` - 8 actions, select one randomly |
| `ACTION_CHKTIME` | 123 | Time inspection | Various time formats (0-5 types) |
| `ACTION_POSTCMD` | 124 | Client transmission | `data`: command serial number |
| `ACTION_BROCASTMSG` | 125 | Server broadcast | `data`: channel, `param`: content |
| `ACTION_MESSAGEBOX` | 126 | Client dialog box | `data`: dialog type (0-99), `param`: message |

### NPC Actions (200-299)
*Need NPC ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_NPC_ATTR` | 201 | Revise/examine NPC attribute | `"attr opt data npc_id"` |
| `ACTION_NPC_ERASE` | 205 | Delete current NPC | Limits dynamic NPC |
| `ACTION_NPC_MODIFY` | 206 | Cross thread revision | `"npc_id attr opt data"` |
| `ACTION_NPC_RESETSYNOWNER` | 207 | Reset faction map master | Faction symbolizing NPC only |
| `ACTION_NPC_FIND_NEXT_TABLE` | 208 | Search next tabulation | `param`: type |
| `ACTION_NPC_ADD_TABLE` | 209 | Add tabulation item | `"type idKey data0 data1 data2 data3 szData"` |
| `ACTION_NPC_DEL_TABLE` | 210 | Delete from tabulation | `"type idKey data0 data1 data2 data3 szData"` |
| `ACTION_NPC_DEL_INVALID` | 211 | Delete expired items | `"type idx"` |
| `ACTION_NPC_TABLE_AMOUNT` | 212 | Check tabulation count | Returns false when >=data |
| `ACTION_NPC_SYS_AUCTION` | 213 | Start system auction | `data`: NPC ID, `param`: prompt |
| `ACTION_NPC_DRESS_SYNCLOTHING` | 214 | Put on faction clothing | |
| `ACTION_NPC_TAKEOFF_SYNCLOTHING` | 215 | Take off faction clothing | |
| `ACTION_NPC_AUCTIONING` | 216 | Judge auction goods | `data`: NPC ID, `param`: type |

### Map Actions (300-399)
*Need current map*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_MAP_MOVENPC` | 301 | Move NPC to map | `data`: NPC ID, `param`: "idMap nPosX nPosY" |
| `ACTION_MAP_MAPUSER` | 302 | Judge map user population | `data`: map ID, `param`: "cmd opt data" |
| `ACTION_MAP_BROCASTMSG` | 303 | Broadcast message | `data`: map ID, `param`: message |
| `ACTION_MAP_DROPITEM` | 304 | Map drop item | `param`: "idItemType idMap nPosX nPosY" |
| `ACTION_MAP_SETSTATUS` | 305 | Set map status | `param`: "mapid status_bit data" |
| `ACTION_MAP_ATTRIB` | 306 | Inspect/revise map attribute | `param`: "field opt data idMap" |
| `ACTION_MAP_REGION_MONSTER` | 307 | Check region monster count | Complex parameters for region inspection |
| `ACTION_MAP_CHANGEWEATHER` | 310 | Change weather | `param`: "Type Intensity Dir Color KeepSecs" |
| `ACTION_MAP_CHANGELIGHT` | 311 | Change map brightness | `param`: "idmap light secs" |
| `ACTION_MAP_MAPEFFECT` | 312 | Map special effect | `param`: "idMap x y EffectName" |
| `ACTION_MAP_CREATEMAP` | 313 | Create map | Complex map creation parameters |
| `ACTION_MAP_FIREWORKS` | 314 | Set off fireworks | |

### Item-Only Actions (400-499)
*Need pUser+pItem ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_ITEM_REQUESTLAYNPC` | 401 | Request lay NPC | `param`: "idNextTask type sort lookface region" |
| `ACTION_ITEM_COUNTNPC` | 402 | Count NPCs | `param`: "field data opt num" |
| `ACTION_ITEM_LAYNPC` | 403 | Create NPC | Complex NPC creation parameters |
| `ACTION_ITEM_DELTHIS` | 498 | Delete current item | **Must be last ACTION** |

### User Item Actions (500-599)
*Need user ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_ITEM_ADD` | 501 | Add item | `data`: itemtype_id, `param`: item attributes |
| `ACTION_ITEM_DEL` | 502 | Delete item | `data`: itemtype_id or name |
| `ACTION_ITEM_CHECK` | 503 | Check item | `data`: itemtype_id or name |
| `ACTION_ITEM_HOLE` | 504 | Weapon hole | `param`: "ChkHole/MakeHole HoleNum" |
| `ACTION_ITEM_REPAIR` | 505 | Equipment repair | `data`: equipment position |
| `ACTION_ITEM_MULTIDEL` | 506 | Delete multiple items | `param`: "idType0 idType1 num" |
| `ACTION_ITEM_MULTICHK` | 507 | Check multiple items | `param`: "idType0 idType1 num" |
| `ACTION_ITEM_LEAVESPACE` | 508 | Check bag space | `param`: "space weight packtype" |

#### Equipment Positions
```
ITEMPOSITION_HELMET = 1
ITEMPOSITION_NECKLACE = 2  
ITEMPOSITION_ARMOR = 3
ITEMPOSITION_WEAPONR = 4
ITEMPOSITION_WEAPONL = 5
ITEMPOSITION_RINGR = 6
ITEMPOSITION_RINGL = 7
ITEMPOSITION_SHOES = 8
ITEMPOSITION_MOUNT = 9
```

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_ITEM_UPEQUIPMENT` | 509 | Equipment operation | `param`: "cmd position" |
| `ACTION_ITEM_EQUIPTEST` | 510 | Equipment quality test | `param`: "equip_pos cmd opt num" |
| `ACTION_ITEM_EQUIPEXIST` | 511 | Check equipment exists | `data`: equipment position |
| `ACTION_ITEM_EQUIPCOLOR` | 512 | Equipment color change | `param`: "equip_pos color" |
| `ACTION_ITEM_FIND` | 513 | Search item | `data`: itemtype_id or name |
| `ACTION_ENCASH_CHIP` | 514 | Exchange chip for money | Money amount in Item Data field |

### NPC-Only Actions (600-699)
*Need not user ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_NPCONLY_CREATENEW_PET` | 601 | Create monster | `param`: "x y generator_id type data name" |
| `ACTION_NPCONLY_DELETE_PET` | 602 | Delete monsters | `param`: "type data name" |
| `ACTION_NPCONLY_MAGICEFFECT` | 603 | Magic effect | `param`: "source_id magic_type magic_level target_id data" |
| `ACTION_NPCONLY_MAGICEFFECT2` | 604 | Ground magic effect | `param`: "source_id magic_type magic_level x y target_id data" |

### Syndicate Actions (700-799)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_SYN_CREATE` | 701 | Create syndicate | `param`: "level money leave_money" |
| `ACTION_SYN_DESTROY` | 702 | Dismiss syndicate | Different effects based on rank |
| `ACTION_SYN_DONATE` | 703 | Donate money | Requires input frame |
| `ACTION_SYN_DEMISE` | 704 | Transfer leadership | `param`: "level" |
| `ACTION_SYN_SET_ASSISTANT` | 705 | Promote vice leader | |
| `ACTION_SYN_CLEAR_RANK` | 706 | Remove rank | |
| `ACTION_SYN_PRESENT_MONEY` | 707 | Gift money to faction | Minimum 10000 |
| `ACTION_SYN_CREATE_SUB` | 708 | Create sub-syndicate | |
| `ACTION_SYN_CHANGE_LEADER` | 709 | Change sub-leader | |
| `ACTION_SYN_COMBINE_SUB` | 710 | Merge sub-faction | |
| `ACTION_SYN_ANTAGONIZE` | 711 | Antagonize faction | |
| `ACTION_SYN_CLEAR_ANTAGONIZE` | 712 | Clear antagonism | |
| `ACTION_SYN_ALLY` | 713 | Form alliance | Requires two leaders teamed |
| `ACTION_SYN_CLEAR_ALLY` | 714 | Clear alliance | |
| `ACTION_SYN_KICKOUT_MEMBER` | 715 | Dismiss member | |
| `ACTION_SYN_CREATENEW_PET` | 716 | Create faction pet | **(Deprecated)** |
| `ACTION_SYN_ATTR` | 717 | Syndicate attributes | `param`: "szField szOpt data syn_id" |
| `ACTION_SYN_CHANGESYN` | 718 | Transfer member | |
| `ACTION_SYN_CHANGE_SUBNAME` | 719 | Change sub-name | Max 6 bytes |
| `ACTION_SYN_FIND_NEXT_SYN` | 720 | Find next syndicate | |
| `ACTION_SYN_FIND_BY_NAME` | 721 | Find by name | |
| `ACTION_SYN_FIND_NEXT_SYNMEMBER` | 722 | Find next member | |
| `ACTION_SYN_SAINT` | 724 | Saint knight promotion | |
| `ACTION_SYN_RANK` | 726 | Modify rank | |
| `ACTION_SYN_UPMEMBERLEVEL` | 728 | Up member level | |
| `ACTION_SYN_ALLOCATE_SYNFUND` | 729 | Allocate faction fund | Max 50% of total |
| `ACTION_SYN_APPLLY_ATTACKSYN` | 730 | Apply attack syndicate | |
| `ACTION_SYN_RENAME` | 731 | Rename faction | Sub-faction only |

### Monster Actions (800-899)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_MST_DROPITEM` | 801 | Monster drop | `param`: "dropitem itemtype" or "dropmoney money" |
| `ACTION_MST_MAGIC` | 802 | Magic inspection | Various magic operations |

### User Attribute Actions (1000-1999)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_USER_ATTR` | 1001 | User attribute modification | `"attr opt data"` |
| `ACTION_USER_FULL` | 1002 | Fill user attribute | `"attr"` (life/mana) |
| `ACTION_USER_CHGMAP` | 1003 | Change map | `"idMap nPosX nPosY bPrisonChk"` |
| `ACTION_USER_RECORDPOINT` | 1004 | Save plot point | `"idMap nMapX nMapY"` |
| `ACTION_USER_HAIR` | 1005 | Hair modification | `"color num"` or `"style num"` |
| `ACTION_USER_CHGMAPRECORD` | 1006 | Change to plot point | |
| `ACTION_USER_CHGLINKMAP` | 1007 | Change to NPC link map | |
| `ACTION_USER_TALK` | 1010 | Send message | `data`: channel, `param`: content |

#### User Attribute Fields
- `life` (+=, ==, <)
- `mana` (+=, ==, <)  
- `money` (+=, ==, <)
- `exp` (+=, ==, <)
- `pk` (+=, ==, <)
- `profession` (==, set, >=, <=)
- `level` (+=, ==, <)
- `force` (+=, ==, <)
- `dexterity` (+=, ==, <)
- `speed` (+=, ==, <)
- `health` (+=, ==, <)
- `soul` (+=, ==, <)
- `rank` (==, <)
- `rankshow` (==, <)
- `iterator` (=, <=, +=, ==)
- `crime` (==, set)
- `gamecard` (==, >=, <=)
- `gamecard2` (==, >=, <=)
- `xp` (+=)
- `metempsychosis` (==, <)
- `mercenary_rank` (==, <, +=)
- `mercenary_exp` (==, <, +=)
- `exploit` (==, <, +=)
- `maxlifepercent` (+=, ==, <)
- `turor_exp` (==, <, +=, =)
- `tutor_level` (==, <, +=, =)
- `syn_proffer` (<, +=, =)
- `maxeudemon` (<, +=, =)

#### Text Attribute Constants
```
_TXTATR_NORMAL = 2000
_TXTATR_ACTION = 2002  // acts
_TXTATR_SYSTEM = 2005  // system  
_TXTATR_TALK = 2007    // talks
_TXTATR_GM = 2011      // GM channel
_TXTATR_WEBPAGE = 2105 // opens URL
```

### Extended User Actions

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_USER_MAGIC` | 1020 | Magic inspection | Various magic operations |
| `ACTION_USER_WEAPONSKILL` | 1021 | Weapon skill | `"check/learn type level"` |
| `ACTION_USER_LOG` | 1022 | Save to GM log | |
| `ACTION_USER_BONUS` | 1023 | Take prize | |
| `ACTION_USER_DIVORCE` | 1024 | Divorce | |
| `ACTION_USER_MARRIAGE` | 1025 | Marriage check | Returns 1=married, 0=unmarried |
| `ACTION_USER_SEX` | 1026 | Sex check | Returns 1=male, 0=female |
| `ACTION_USER_EFFECT` | 1027 | Trigger special effect | `"opt effect"` |
| `ACTION_USER_TASKMASK` | 1028 | Task mask operation | `"opt idx"` |
| `ACTION_USER_MEDIAPLAY` | 1029 | Media broadcast | `"opt media"` |
| `ACTION_USER_SUPERMANLIST` | 1030 | Query superman list | `"idNextTask number"` |
| `ACTION_USER_CHKIN_CARD` | 1031 | Check in game card | |
| `ACTION_USER_CHKOUT_CARD` | 1032 | Check out game card | |
| `ACTION_USER_CREATEMAP` | 1033 | Create user map | Complex parameters |
| `ACTION_USER_ENTER_HOME` | 1034 | Enter own home | |
| `ACTION_USER_ENTER_MATE_HOME` | 1035 | Enter spouse home | |
| `ACTION_USER_CHKIN_CARD2` | 1036 | Check in game card 2 | |
| `ACTION_USER_CHKOUT_CARD2` | 1037 | Check out game card 2 | |
| `ACTION_USER_FLY_NEIGHBOR` | 1038 | Fly to neighbor | `"serial"` |
| `ACTION_USER_UNLEARN_MAGIC` | 1039 | Unlearn magic | `"type1 type2..."` |
| `ACTION_USER_REBIRTH` | 1040 | Rebirth | `"nProf nLook"` |
| `ACTION_USER_WEBPAGE` | 1041 | Open webpage | `"http://..."` |
| `ACTION_USER_BBS` | 1042 | BBS message | |
| `ACTION_USER_UNLEARN_SKILL` | 1043 | Unlearn weapon skills | |
| `ACTION_USER_DROP_MAGIC` | 1044 | Drop magic skill | `"type1 type2..."` |
| `ACTION_USER_OPEN_DIALOG` | 1046 | Open dialog | `data`: dialog ID |
| `ACTION_USER_CHGMAP_REBORN` | 1047 | Change to reborn point | |
| `ACTION_USER_DEL_WPG_BADGE` | 1049 | Delete WPG badge | |
| `ACTION_USER_CHK_WPG_BADGE` | 1050 | Check WPG badge | |
| `ACTION_USER_TAKESTUDENTEXP` | 1051 | Take student experience | |
| `ACTION_USER_CHGTO_MAINMAP` | 1052 | Change to main map | |
| `ACTION_USER_CHGTO_RANDOMPOS` | 1053 | Change to random position | |

### Task System Actions (1080-1083)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_USER_TASK_MANAGER` | 1080 | Task management | `data`: mission number, `param`: operation |
| `ACTION_USER_TASK_OPE` | 1081 | Task operation | `data`: mission number, `param`: "ope opt data" |
| `ACTION_USER_TASK_LOCALTIME` | 1082 | Task local time | `data`: mission number, `param`: second number |
| `ACTION_USER_TASK_FIND` | 1083 | Task inquiry | `param`: find parameters |

### Team Actions (1101-1501)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_TEAM_BROADCAST` | 1101 | Team broadcast | `param`: message |
| `ACTION_TEAM_ATTR` | 1102 | Team attribute check | `param`: "field opt data" |
| `ACTION_TEAM_LEAVESPACE` | 1103 | Team bag space check | `param`: "space weight packtype" |
| `ACTION_TEAM_ITEM_ADD` | 1104 | Team add item | `data`: itemtype_id |
| `ACTION_TEAM_ITEM_DEL` | 1105 | Team delete item | `data`: itemtype_id |
| `ACTION_TEAM_ITEM_CHECK` | 1106 | Team check item | `data`: itemtype_id |
| `ACTION_TEAM_CHGMAP` | 1107 | Team change map | `param`: "mapid x y" |
| `ACTION_TEAM_CHK_ISLEADER` | 1501 | Check if team leader | No parameters |

### Event Actions (2000-2099)
*Need not any ptr*

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_EVENT_SETSTATUS` | 2001 | Set map status | `param`: "mapid status_bit data" |
| `ACTION_EVENT_DELNPC_GENID` | 2002 | Delete monster | **(Deprecated)** |
| `ACTION_EVENT_COMPARE` | 2003 | Compare attributes | `"data1 opt data2"` |
| `ACTION_EVENT_COMPARE_UNSIGNED` | 2004 | Compare unsigned | `"data1 opt data2"` |
| `ACTION_EVENT_CHANGEWEATHER` | 2005 | Change weather | `param`: weather parameters |
| `ACTION_EVENT_CREATEPET` | 2006 | Create monster | `param`: monster parameters |
| `ACTION_EVENT_CREATENEW_NPC` | 2007 | Create new NPC | `param`: NPC parameters |
| `ACTION_EVENT_COUNTMONSTER` | 2008 | Count monsters | `param`: "idMap field data opt num" |
| `ACTION_EVENT_DELETEMONSTER` | 2009 | Delete monsters | `param`: "idMap type data name" |
| `ACTION_EVENT_BBS` | 2010 | System BBS message | `param`: message content |
| `ACTION_EVENT_ERASE` | 2011 | Delete NPC | `param`: "idMap type" |

### Trap Actions (2100-2199)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_TRAP_CREATE` | 2101 | Create trap | `param`: "type look owner_id map_id pos_x pos_y data" |
| `ACTION_TRAP_ERASE` | 2102 | Delete trap | Current trap |
| `ACTION_TRAP_COUNT` | 2103 | Count traps | `param`: "map_id pos_x pos_y pos_cx pos_cy count type" |
| `ACTION_TRAP_ATTR` | 2104 | Trap attributes | `param`: "id field opt num" |

### Wanted List Actions (3000-3099)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_WANTED_NEXT` | 3001 | Search wanted | Reads to TASK_ITERATOR |
| `ACTION_WANTED_NAME` | 3002 | Wanted name | Returns name to pszAccept |
| `ACTION_WANTED_BONUTY` | 3003 | Wanted bounty | Returns amount to pszAccept |
| `ACTION_WANTED_NEW` | 3004 | New wanted record | |
| `ACTION_WANTED_ORDER` | 3005 | Receive wanted | |
| `ACTION_WANTED_CANCEL` | 3006 | Cancel wanted | |
| `ACTION_WANTED_MODIFYID` | 3007 | Modify wanted ID | |
| `ACTION_WANTED_SUPERADD` | 3008 | Add to bounty | |
| `ACTION_POLICEWANTED_NEXT` | 3010 | Search police wanted | |
| `ACTION_POLICEWANTED_ORDER` | 3011 | Police order | |
| `ACTION_POLICEWANTED_CHECK` | 3012 | Check if wanted | |

### Magic Actions (4000-4099)

| Action | ID | Description | Parameters |
|--------|----|--------------|---------| 
| `ACTION_MAGIC_ATTACHSTATUS` | 4001 | Attach status | `param`: "status power secs times" |
| `ACTION_MAGIC_ATTACK` | 4002 | Magic attack | `data`: magictype, `param`: "magiclevel" |

## Practical Example: Random Probability Chains

*Now forcer really did all the work by translating some old server sources long ago :handsdown:, but it seems that not everyone jumped in found all the cool things you can do in cq_action. :rtfm: This is an explanation of one of those cool things:*

Let's take a look at the candy db for some cq_action with type 121 or 122:

| id | id_next | id_nextfail | type | data | param |
|----|---------|-------------|------|------|-------|
| 660000 | 660001 | 660018 | 121 | 0 | 1 5 |
| 660001 | 0 | 0 | 122 | 0 | 660039 660039 660040 660040 660041 660041 660042 660042 |
| 660002 | 0 | 0 | 122 | 0 | 660030 660030 660031 660031 660032 660032 660033 660033 |
| 660018 | 660002 | 660019 | 121 | 0 | 1 4 |
| 660019 | 660003 | 660020 | 121 | 0 | 1 3 |

So I will explain line by line:

In the first line, you have a **1 in 5 chance** of passing. If you pass, you go to line 660001. If you fail (4 in 5 chance) you go to line 660018.

Let's say you beat the odds and you passed. You go to line 660001. There the server picks randomly from the list of cq_action to go to from the ids given in the parameter separated by spaces. As you can see, you have a **2 in 8 chance** of getting any one of the four id actions given. You can use your imagination where these actions go to. They each could be, for example, a different amount of ep awarded.

So let's say you didn't beat the 1 in 5 odds and you didn't pass the first line. You get sent to line 660018. Now you get a **1 in 4 chance** to pass. Yay! If you pass, it is the same scenario as you faced if you passed the first line, but with different actions. Perhaps here they might have PPs instead of EPs on the first line. If you were to fail, you would get sent to 660019 which is a **1 in 3 chance**, etc, etc, etc.

So now that I spelled it out for everyone, you should all be able to use these effectively and all make super creative and unique events from now on :D

---

*Original documentation preserved for reference. Translation artifacts and terminology maintained as per historical record.*