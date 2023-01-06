using System.Runtime.InteropServices;
using System.Timers;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;
using Org.BouncyCastle.Ocsp;

namespace MagnumOpus.Helpers
{
    public static class CqProcessor
    {
        public static cq_npc? GetNpc(int npcId)
        {
            using var ctx = new SquigglyContext();
            return ctx.cq_npc.Find((long)npcId);
        }
        public static cq_task? GetTask(long taskId)
        {
            using var ctx = new SquigglyContext();
            return ctx.cq_task.Find(taskId);
        }
        public static cq_action? GetAction(long? actionId)
        {
            if (actionId == null)
                return null;

            using var ctx = new SquigglyContext();
            return ctx.cq_action.Find(actionId);
        }

        public static void Process(in PixelEntity ntt, cq_task task)
        {
            var action = GetAction(task.id_next);
            if (action != null)
                CqActionProcessor.Process(ntt, action);
        }

        public static void Process(in PixelEntity ntt, cq_action? action)
        {
            if (action == null)
                return;

            CqActionProcessor.Process(ntt, action);
        }
    }
    public static class CqActionProcessor
    {
        private static readonly Dictionary<string, Func<long, long, long>> AttrOpts = new()
        {
            { "+=", (a, b) => a += b },
            { "-=", (a, b) => a -= b },
        };
        private static readonly Dictionary<string, Func<long, long, bool>> BooleanAttrOpts = new()
        {
            { ">", (a, b) => a > b },
            { "<", (a, b) => a < b },
            { "==", (a, b) => a == b },
            { ">=", (a, b) => a >= b },
            { "<=", (a, b) => a <= b },
        };

        public static readonly Dictionary<string, Func<PixelEntity, long, string, bool>> AttrVals = new()
        {
            { "money", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<InventoryComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func(x.Money, targetVal);
                        if(result >= 0)
                        {
                            x.Money = (uint)result;
                            var pak = MsgUserAttrib.Create(ntt.NetId, x.Money, MsgUserAttribType.InvMoney);
                            ntt.NetSync(ref pak);
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc(x.Money, targetVal);

                    return false;
                }
            },
            { "virtue", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<VirtuePointComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func(x.Points, targetVal);
                        if(result >= 0)
                        {
                            x.Points = (uint)result;
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc(x.Points, targetVal);
                    return false;
                }
            },
            { "profession", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<ProfessionComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func((long)x.Class, targetVal);
                        if(result >= 0)
                        {
                            x.Class = (ClasseName)result;
                            var pak = MsgUserAttrib.Create(ntt.NetId, (byte)x.Class, MsgUserAttribType.Class);
                            ntt.NetSync(ref pak);
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc((long)x.Class, targetVal);
                    return false;
                }
            },
            { "level", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<LevelComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func((long)x.Level, targetVal);
                        if(result >= 0)
                        {
                            x.Level = (byte)result;
                            var pak = MsgUserAttrib.Create(ntt.NetId, x.Level, MsgUserAttribType.Level);
                            ntt.NetSync(ref pak);
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc((long)x.Level, targetVal);
                    return false;
                }
            },
            { "life", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<HealthComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func((long)x.Health, targetVal);
                        if(result >= 0)
                        {
                            x.Health = (byte)result;
                            var pak = MsgUserAttrib.Create(ntt.NetId, (ulong)x.Health, MsgUserAttribType.Health);
                            ntt.NetSync(ref pak);
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc((long)x.Health, targetVal);
                    return false;
                }
            },
            { "metempsychosis", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<RebornComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func((long)x.Count, targetVal);
                        if(result >= 0)
                        {
                            x.Count = (byte)result;
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc((long)x.Count, targetVal);
                    return false;
                }
            }
        };

        public static long Process(in PixelEntity ntt, cq_action action)
        {
            var taskType = (TaskActionType)action.type;

            switch (taskType)
            {
                case TaskActionType.ACTION_MENUTEXT:
                    {
                        ref readonly var tac = ref ntt.Get<TaskComponent>();
                        var text = action.param.Replace("~", " ").Trim();
                        var textPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Text, text);
                        var textMem = Co2Packet.Serialize(ref textPacket, textPacket.Size);
                        ntt.NetSync(textMem);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENULINK:
                    {
                        ref var tac = ref ntt.Get<TaskComponent>();
                        tac.OptionCount++;
                        var text = action.param.Trim().Split(' ')[0];
                        var optionId = int.Parse(action.param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        var optionPacket = MsgTaskDialog.Create(in tac.Npc, tac.OptionCount, MsgTaskDialogAction.Link, text);
                        var optionMem = Co2Packet.Serialize(ref optionPacket, optionPacket.Size);
                        tac.Options[tac.OptionCount] = (int)optionId;
                        ntt.NetSync(optionMem);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUEDIT:
                    {
                        ref var tac = ref ntt.Get<TaskComponent>();
                        tac.OptionCount++;
                        var text = action.param.Trim().Split(' ')[2];
                        var optionId = int.Parse(action.param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        var optionPacket = MsgTaskDialog.Create(in tac.Npc, tac.OptionCount, MsgTaskDialogAction.Edit, text);
                        var optionMem = Co2Packet.Serialize(ref optionPacket, optionPacket.Size);
                        tac.Options[tac.OptionCount] = (int)optionId;
                        ntt.NetSync(optionMem);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUPIC:
                    {
                        ref readonly var tac = ref ntt.Get<TaskComponent>();
                        var facePacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Picture);
                        var faceId = byte.Parse(action.param.Trim().Split(' ')[2]);
                        facePacket.Avatar = faceId;
                        var faceMem = Co2Packet.Serialize(ref facePacket, facePacket.Size);
                        ntt.NetSync(faceMem);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUCREATE:
                    {
                        ref var tac = ref ntt.Get<TaskComponent>();
                        tac.OptionCount = 0;
                        var showPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Create);
                        var showMem = Co2Packet.Serialize(ref showPacket, showPacket.Size);
                        ntt.NetSync(showMem);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_ATTR:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var attribute = parameters[0];
                        var operation = parameters[1];
                        var targetVal = long.Parse(parameters[2]);

                        if (!AttrVals.TryGetValue(attribute, out var func))
                        {
                            FConsole.WriteLine($"Unknown attribute {attribute}");
                            return action.id_nextfail;
                        }
                        var result = func(ntt, targetVal, operation);
                        FConsole.WriteLine($"if ({attribute} {operation} {targetVal}) -> {result}");
                        return result ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_USER_CHGMAP:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var mapId = int.Parse(parameters[0]);
                        var x = int.Parse(parameters[1]);
                        var y = int.Parse(parameters[2]);

                        var tpc = new TeleportComponent(ntt.Id, (ushort)x, (ushort)y, (ushort)mapId);
                        ntt.Add(ref tpc);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_CHGMAPRECORD:
                    {
                        if (!ntt.Has<RecordPointComponent>())
                        {
                            ref readonly var rpc = ref ntt.Get<RecordPointComponent>();
                            var tpc = new TeleportComponent(ntt.Id, rpc.X, rpc.Y, rpc.Map);
                            ntt.Add(ref tpc);
                            return action.id_next;
                        }
                        return action.id_nextfail;
                    }
                case TaskActionType.ACTION_POLICEWANTED_CHECK:
                    {
                        ref readonly var pkc = ref ntt.Get<PkPointComponent>();
                        // idnext if wanted, idnextfail if not
                        return pkc.Points >= 100 ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_USER_RECORDPOINT:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var map = ushort.Parse(parameters[0]);
                        var x = ushort.Parse(parameters[1]);
                        var y = ushort.Parse(parameters[2]);

                        var rpc = new RecordPointComponent(ntt.Id, x, y, map);
                        ntt.Add(ref rpc);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_MAGIC:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var op = parameters[0];
                        var skillId = ushort.Parse(parameters[1]);

                        ref var sbc = ref ntt.Get<SpellBookComponent>();

                        if (op == "check")
                        {
                            if (sbc.Spells.ContainsKey(skillId))
                                return action.id_next;
                            return action.id_nextfail;
                        }
                        if (op == "learn")
                        {
                            sbc.Spells.Add(skillId, (0, 0, 0));
                            var msg = MsgSkill.Create(skillId, 0, 0);
                            ntt.NetSync(ref msg);
                        }

                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_HAIR:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var style = parameters[1];
                        ref var hair = ref ntt.Get<BodyComponent>();
                        var color = (hair.Hair / 100) * 100;
                        hair.Hair = (ushort)(color + int.Parse(style));

                        var msg = MsgUserAttrib.Create(ntt.NetId, hair.Hair, MsgUserAttribType.HairStyle);
                        ntt.NetSync(ref msg, true);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_MEDIAPLAY:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var media = parameters[1];
                        var msg = MsgName.Create(ntt.NetId, media, (byte)MsgNameType.Sound);
                        ntt.NetSync(in msg);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_EFFECT:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var effect = parameters[1];
                        var msg = MsgName.Create(ntt.NetId, effect, (byte)MsgNameType.RoleEffect);
                        ntt.NetSync(in msg, true);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_RAND:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var a = int.Parse(parameters[0]);
                        var b = int.Parse(parameters[1]);

                        var chance = a / (float)b;
                        var result = Random.Shared.NextSingle();
                        if (result < chance)
                            return action.id_next;

                        return action.id_nextfail;
                    }
                case TaskActionType.ACTION_RANDACTION:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var idx = Random.Shared.Next(0, 8);
                        var next = long.Parse(parameters[idx]);
                        return next;
                    }
                case TaskActionType.ACTION_MAP_MOVENPC:
                    {
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MST_DROPITEM:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var itemId = int.Parse(parameters[1]);
                        
                        ref var itemNtt = ref PixelWorld.CreateEntity(EntityType.Item);
                        var itemComp = new ItemComponent(itemNtt.Id, itemId,0,0,0,0,0,0,0,0,0,0);
                        itemNtt.Add(ref itemComp);

                        var drc = new DropRequestComponent(ntt.Id, itemNtt.NetId);
                        ntt.Add(ref drc);

                        FConsole.WriteLine($"DropRequestComponent added to {ntt.Id} for item {itemId}");

                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_TALK:
                    {
                        var msg = MsgText.Create("SYSTEM", "ALLUSERS", action.param.Trim(), (MsgTextType)action.data);
                        var srz = Co2Packet.Serialize(ref msg, msg.Size);
                        ntt.NetSync(in srz, true);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_ITEM_CHECK:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            ref readonly var item = ref inv.Items[i].Get<ItemComponent>();
                            if (item.Id == action.data)
                                return action.id_next;
                        }
                        return action.id_next; // ERROR: should be id_nextfail, just for testing its not
                    }
                case TaskActionType.ACTION_ITEM_MULTICHK:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var startId = int.Parse(parameters[0]);
                        var endId = int.Parse(parameters[1]);
                        var chkCount = int.Parse(parameters[2]);
                        var chkRange = Enumerable.Range(startId, endId - startId + 1).ToArray();

                        var count = 0;

                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            ref readonly var item = ref inv.Items[i].Get<ItemComponent>();
                            if (chkRange.Contains(item.Id))
                                count++;
                        }

                        if (count >= chkCount)
                            return action.id_next;

                        return action.id_next; // ERROR: should be id_nextfail, just for testing its not
                    }
                case TaskActionType.ACTION_ITEM_MULTIDEL:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var startId = int.Parse(parameters[0]);
                        var endId = int.Parse(parameters[1]);
                        var chkCount = int.Parse(parameters[2]);
                        var chkRange = Enumerable.Range(startId, endId - startId + 1).ToArray();

                        var count = 0;

                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            ref readonly var item = ref inv.Items[i].Get<ItemComponent>();
                            
                            if (chkRange.Contains(item.Id))
                            {
                                var msg = MsgItem.Create(ntt.NetId, inv.Items[i].NetId,inv.Items[i].NetId,PixelWorld.Tick, MsgItemType.RemoveInventory);
                                ntt.NetSync(ref msg);
                                count++;
                                inv.Items[i] = default;
                            }
                            if (count >= chkCount)
                                return action.id_next;
                        }
                        return action.id_next; // ERROR: should be id_nextfail, just for testing its not
                    }
                    case TaskActionType.ACTION_ITEM_LEAVESPACE:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var chkCount = action.data;

                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].NetId == 0)
                                chkCount--;
                        }

                        if (chkCount <= 0)
                            return action.id_next;
                        
                        return action.id_next;
                    }
                    case TaskActionType.ACTION_ITEM_ADD:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var itemId = action.data;

                        ref var itemNtt = ref PixelWorld.CreateEntity(EntityType.Item);
                        var item = new ItemComponent(itemNtt.Id, itemId, 1,1,0,0,0,0,0,0,RebornItemEffect.None, 0);
                        itemNtt.Add(ref item);
                        
                        for(int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].NetId != 0)
                                continue;
                            
                            inv.Items[i] = itemNtt;
                            var msg = MsgItemInformation.Create(in itemNtt, MsgItemInfoAction.AddItem, MsgItemPosition.Inventory);
                            ntt.NetSync(ref msg);
                            return action.id_next;
                        }

                        return action.id_nextfail;
                    }
                    case TaskActionType.ACTION_ITEM_DEL:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var itemId = action.data;

                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].NetId != itemId)
                                continue;

                            var removeInv = MsgItem.Create(inv.Items[i].NetId, inv.Items[i].NetId, inv.Items[i].NetId, PixelWorld.Tick, Enums.MsgItemType.RemoveInventory);
                            ntt.NetSync(ref removeInv);
                            inv.Items[i] = default;
                            return action.id_nextfail;
                        }

                        return action.id_next; // ERROR: should be id_nextfail, just for testing its not
                    }
                default:
                    FConsole.WriteLine($"[FAIL] Unknown task type: {taskType}");
                    FConsole.WriteLine($"| - Task:     {action.id}");
                    FConsole.WriteLine($"| - Param:    {action.param.Trim()}");
                    FConsole.WriteLine($"| - Data:     {action.data}");
                    FConsole.WriteLine($"| - Next:     {action.id_next}");
                    FConsole.WriteLine($"| - Fail:     {action.id_nextfail}");
                    return action.id_nextfail;
            }
        }
    }
}