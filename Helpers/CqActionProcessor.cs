using System.Globalization;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Helpers
{
    public static class CqProcessor
    {
        public static cq_npc GetNpc(int npcId)
        {
            using var ctx = new SquigglyContext();
            return ctx.cq_npc.Find((long)npcId);
        }

        public static cq_task GetTask(long taskId)
        {
            using var ctx = new SquigglyContext();
            return ctx.cq_task.Find(taskId);
        }

        public static cq_action GetAction(long actionId)
        {
            using var ctx = new SquigglyContext();
            return ctx.cq_action.Find(actionId);
        }
    }
    public static class CqActionProcessor
    {
        private static readonly bool _trace = false;

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

        public static readonly Dictionary<string, Func<NTT, long, string, bool>> AttrVals = new()
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
                            var pak = MsgUserAttrib.Create(ntt.NetId, x.Money, MsgUserAttribType.MoneyInventory);
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
                    ref var x = ref ntt.Get<ClassComponent>();
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
                        var result = func(x.Health, targetVal);
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
                        return boolFunc(x.Health, targetVal);
                    return false;
                }
            },
            { "metempsychosis", (ntt, targetVal, op) =>
                {
                    ref var x = ref ntt.Get<RebornComponent>();
                    if(AttrOpts.TryGetValue(op, out var func))
                    {
                        var result = func(x.Count, targetVal);
                        if(result >= 0)
                        {
                            x.Count = (byte)result;
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc(x.Count, targetVal);
                    return false;
                }
            }
        };

        public static long Process(in NTT ntt, in NTT trigger, cq_action action)
        {
            if (action == null)
                return 0;

            var taskType = (TaskActionType)action.type;

            switch (taskType)
            {
                case TaskActionType.ACTION_MENUTEXT:
                    {
                        ref readonly var tac = ref ntt.Get<CqTaskComponent>();
                        var text = action.param.Replace("~", " ").Trim();
                        var textPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Text, text);
                        ntt.NetSync(ref textPacket);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENULINK:
                    {
                        if (!ntt.Has<CqTaskComponent>())
                        {
                            var tacComp = new CqTaskComponent(ntt.Id, 0);
                            ntt.Set(ref tacComp);
                        }
                        ref var tac = ref ntt.Get<CqTaskComponent>();
                        tac.OptionCount++;
                        var text = action.param.Trim().Split(' ')[0];
                        var optionId = int.Parse(action.param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        var optionPacket = MsgTaskDialog.Create(in tac.Npc, tac.OptionCount, MsgTaskDialogAction.Link, text);
                        tac.Options[tac.OptionCount] = (int)optionId;
                        ntt.NetSync(ref optionPacket);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUEDIT:
                    {
                        ref var tac = ref ntt.Get<CqTaskComponent>();
                        tac.OptionCount++;
                        var text = action.param.Trim().Split(' ')[2];
                        var optionId = int.Parse(action.param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                        var optionPacket = MsgTaskDialog.Create(in tac.Npc, tac.OptionCount, MsgTaskDialogAction.Edit, text);
                        tac.Options[tac.OptionCount] = (int)optionId;
                        ntt.NetSync(ref optionPacket);
                        FConsole.WriteLine(text);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUPIC:
                    {
                        ref readonly var tac = ref ntt.Get<CqTaskComponent>();
                        var facePacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Picture);
                        var faceId = byte.Parse(action.param.Trim().Split(' ')[2]);
                        facePacket.Avatar = faceId;
                        ntt.NetSync(ref facePacket);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MENUCREATE:
                    {
                        ref var tac = ref ntt.Get<CqTaskComponent>();
                        tac.OptionCount = 0;
                        var showPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Create);
                        ntt.NetSync(ref showPacket);
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
                            if (_trace)
                                FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> Unknown attribute {attribute} -> {action.id_nextfail}");
                            return action.id_nextfail;
                        }
                        var result = func(ntt, targetVal, operation);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> if ({attribute} {operation} {targetVal}) -> {result} -> {(result ? action.id_next : action.id_nextfail)}");
                        return result ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_USER_CHGMAP:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var mapId = int.Parse(parameters[0]);
                        var x = int.Parse(parameters[1]);
                        var y = int.Parse(parameters[2]);

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {x},{y},{mapId} -> {action.id_next}");

                        var tpc = new TeleportComponent(ntt.Id, (ushort)x, (ushort)y, (ushort)mapId);
                        ntt.Set(ref tpc);
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_CHGMAPRECORD:
                    {
                        ref readonly var rpc = ref ntt.Get<RecordPointComponent>();
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {rpc.X},{rpc.Y},{rpc.Map} {(rpc.EntityId != 0 ? action.id_next : action.id_nextfail)}");

                        if (rpc.EntityId != 0)
                        {
                            var tpc = new TeleportComponent(ntt.Id, rpc.X, rpc.Y, rpc.Map);
                            ntt.Set(ref tpc);
                            return action.id_next;
                        }
                        return action.id_nextfail;
                    }
                case TaskActionType.ACTION_POLICEWANTED_CHECK:
                    {
                        ref readonly var pkc = ref ntt.Get<PkPointComponent>();
                        var wanted = pkc.Points >= 100;
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {wanted} -> {(wanted ? action.id_next : action.id_nextfail)}");
                        return wanted ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_USER_RECORDPOINT:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var map = ushort.Parse(parameters[0]);
                        var x = ushort.Parse(parameters[1]);
                        var y = ushort.Parse(parameters[2]);

                        var rpc = new RecordPointComponent(ntt.Id, x, y, map);
                        ntt.Set(ref rpc);

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {x},{y},{map} -> {action.id_next}");

                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_MAGIC:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var op = parameters[0];
                        var skillId = ushort.Parse(parameters[1]);

                        ref var sbc = ref ntt.Get<SpellBookComponent>();
                        var checkResult = sbc.Spells.ContainsKey(skillId);

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {op} {skillId} -> {(op == "check" ? checkResult : !checkResult)} -> {(op == "check" ? (checkResult ? action.id_next : action.id_nextfail) : (!checkResult ? action.id_next : action.id_nextfail))}");

                        if (op == "check")
                        {
                            if (checkResult)
                                return action.id_next;
                            return action.id_nextfail;
                        }
                        else if (op == "learn" && !checkResult)
                        {
                            sbc.Spells.Add(skillId, (0, 0, 0));
                            var msg = MsgSkill.Create(skillId, 0, 0);
                            ntt.NetSync(ref msg);
                            return action.id_next;
                        }

                        return action.id_nextfail;
                    }
                case TaskActionType.ACTION_USER_HAIR:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var style = parameters[1];
                        ref var head = ref ntt.Get<HeadComponent>();
                        var color = head.Hair / 100 * 100;
                        head.Hair = (ushort)(color + int.Parse(style));
                        var msg = MsgUserAttrib.Create(ntt.NetId, head.Hair, MsgUserAttribType.HairStyle);
                        ntt.NetSync(ref msg, true);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> Color: {color}, Style: {style}) -> {head.Hair} -> {action.id_next}");
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_MEDIAPLAY:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var media = parameters[1];
                        var msg = MsgName.Create(ntt.NetId, media, (byte)MsgNameType.Sound);
                        ntt.NetSync(ref msg);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {media} -> {action.id_next}");
                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_EFFECT:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var effect = parameters[1];
                        var msg = MsgName.Create(ntt.NetId, effect, (byte)MsgNameType.RoleEffect);
                        ntt.NetSync(ref msg, true);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {effect} -> {action.id_next}");
                        return action.id_next;
                    }
                case TaskActionType.ACTION_RAND:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var a = int.Parse(parameters[0]);
                        var b = int.Parse(parameters[1]);

                        var chance = a / (float)b;
                        var result = Random.Shared.NextSingle();

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {a}/{b} -> {chance * 100}% -> {result * 100}% -> {(result < chance ? "Success" : "Fail")} -> {(result < chance ? action.id_next : action.id_nextfail)}");

                        if (result < chance)
                            return action.id_next;

                        return action.id_nextfail;
                    }
                case TaskActionType.ACTION_RANDACTION:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var idx = Random.Shared.Next(0, 8);
                        var next = long.Parse(parameters[idx]);

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {action.param.Trim()} -> Random Dice: {idx} -> {next}");
                        return next;
                    }
                case TaskActionType.ACTION_MAP_MOVENPC:
                    {
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> NOT CODED YET -> {action.id_next}");
                        return action.id_next;
                    }
                case TaskActionType.ACTION_MST_DROPITEM:
                    {
                        var parameters = action.param.Trim().Split(' ');
                        var itemId = int.Parse(parameters[1]);
                        var itemExists = Collections.ItemType.TryGetValue(itemId, out var entry);

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {itemId} -> {(itemExists ? "Exists" : "Fail")} -> {(itemExists ? action.id_next : action.id_nextfail)}");

                        if (!itemExists)
                            return action.id_nextfail;

                        ref var itemNtt = ref NttWorld.CreateEntity(EntityType.Item);

                        var dura = (ushort)Random.Shared.Next(0, entry.AmountLimit);
                        var itemComp = new ItemComponent(itemNtt.Id, itemId, dura, entry.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
                        itemNtt.Set(ref itemComp);

                        ref var inv = ref ntt.Get<InventoryComponent>();
                        var idx = Array.FindIndex(inv.Items, x => x.Id == 0);
                        if (idx == -1)
                            return action.id_nextfail;

                        inv.Items[idx] = itemNtt;

                        var drc = new RequestDropItemComponent(ntt.Id, in itemNtt);
                        ntt.Set(ref drc);

                        return action.id_next;
                    }
                case TaskActionType.ACTION_USER_TALK:
                    {
                        var msg = MsgText.Create("SYSTEM", "ALLUSERS", action.param.Trim(), (MsgTextType)action.data);
                        ntt.NetSync(ref msg, true);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {action.param.Trim()} | {(MsgTextType)action.data} -> {action.id_next}");
                        return action.id_next;
                    }
                case TaskActionType.ACTION_ITEM_CHECK:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        bool found = false;
                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            ref readonly var item = ref inv.Items[i].Get<ItemComponent>();
                            if (item.Id != action.data)
                                continue;

                            found = true;
                            break;
                        }

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {action.data} -> {(found ? "Found/Success" : "NotFound/Fail")} -> {(found ? action.id_next : action.id_nextfail)}");

                        return action.id_nextfail;
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

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {action.param.Trim()} -> {(count >= chkCount ? "Found/Success" : "NotFound/Fail")} -> {(count >= chkCount ? action.id_next : action.id_nextfail)}");
                        return count >= chkCount ? action.id_next : action.id_nextfail;
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
                                var msg = MsgItem.Create(ntt.NetId, inv.Items[i].NetId, inv.Items[i].NetId, MsgItemType.RemoveInventory);
                                ntt.NetSync(ref msg);
                                count++;
                                inv.Items[i] = default;
                            }
                            if (count >= chkCount)
                                break;
                        }

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {action.param.Trim()} -> {(count >= chkCount ? "Found/Success" : "NotFound/Fail")} -> {(count >= chkCount ? action.id_next : action.id_nextfail)}");
                        return count >= chkCount ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_ITEM_LEAVESPACE:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var chkCount = action.data;
                        var count = 0;

                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].NetId == 0)
                                count++;
                        }

                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {chkCount} -> {(count >= chkCount ? "Success" : "Fail")} -> {(count >= chkCount ? action.id_next : action.id_nextfail)}");
                        return count >= chkCount ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_ITEM_ADD:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var itemId = action.data;
                        var itemFound = Collections.ItemType.TryGetValue(itemId, out var itemType);

                        var idx = Array.IndexOf(inv.Items, default);
                        if (_trace)
                            FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {itemId} -> {(idx != -1 && itemFound ? "Success" : "Fail")} -> {(idx != -1 && itemFound ? action.id_next : action.id_nextfail)}");

                        if (idx != -1)
                        {
                            ref var itemNtt = ref NttWorld.CreateEntity(EntityType.Item);
                            var item = new ItemComponent(itemNtt.Id, itemId, itemType.Amount, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, RebornItemEffect.None, 0);
                            itemNtt.Set(ref item);
                            inv.Items[idx] = itemNtt;
                            var msg = MsgItemInformation.Create(in itemNtt, MsgItemInfoAction.AddItem, MsgItemPosition.Inventory);
                            ntt.NetSync(ref msg);
                        }

                        return idx != -1 ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_ITEM_DEL:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var itemId = trigger.Id;
                        var foundIdx = -1;

                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].Id != itemId)
                                continue;

                            foundIdx = i;
                            break;
                        }

                        if (foundIdx != -1)
                        {
                            var removeInv = MsgItem.Create(inv.Items[foundIdx].NetId, inv.Items[foundIdx].NetId, inv.Items[foundIdx].NetId, MsgItemType.RemoveInventory);
                            ntt.NetSync(ref removeInv);
                            inv.Items[foundIdx] = default;
                            if (_trace)
                                FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {itemId} -> {(foundIdx != -1 ? "Success" : "Fail")} -> {(foundIdx != -1 ? action.id_next : action.id_nextfail)}");
                        }

                        return foundIdx != -1 ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_ITEM_DELTHIS:
                    {
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();
                        var itemId = trigger.Id;
                        var foundIdx = -1;

                        for (int i = 0; i < inv.Items.Length; i++)
                        {
                            if (inv.Items[i].Id != itemId)
                                continue;

                            foundIdx = i;
                            break;
                        }

                        if (foundIdx != -1)
                        {
                            var removeInv = MsgItem.Create(inv.Items[foundIdx].NetId, inv.Items[foundIdx].NetId, inv.Items[foundIdx].NetId, MsgItemType.RemoveInventory);
                            ntt.NetSync(ref removeInv);
                            inv.Items[foundIdx] = default;
                            if (_trace)
                                FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {itemId} -> {(foundIdx != -1 ? "Success" : "Fail")} -> {(foundIdx != -1 ? action.id_next : action.id_nextfail)}");
                        }

                        return foundIdx != -1 ? action.id_next : action.id_nextfail;
                    }
                case TaskActionType.ACTION_CHKTIME:
                    {
                        if (action.data == 3)
                        {
                            var param = action.param.Trim().Split(' ');
                            var startDayOfWeek = int.Parse(param[0]);
                            var startTime = param[1];
                            var endDayOfWeek = int.Parse(param[2]);
                            var endTime = param[3];
                            var isNow = false;

                            if (((int)DateTime.Now.DayOfWeek) >= startDayOfWeek && ((int)DateTime.Now.DayOfWeek) <= endDayOfWeek)
                            {
                                var startDT = DateTime.ParseExact(startTime, "H:mm", null, DateTimeStyles.None);
                                var endDT = DateTime.ParseExact(endTime, "H:mm", null, DateTimeStyles.None);
                                isNow = DateTime.Now.Hour >= startDT.Hour && DateTime.Now.Hour <= endDT.Hour;
                            }

                            return isNow ? action.id_next : action.id_nextfail;
                        }
                        else
                        {
                            var timestamps = action.param.Trim().Split(' ');
                            var start = string.Join(' ', timestamps[0..2]);
                            var end = string.Join(' ', timestamps[2..4]);

                            var startTime = DateTime.ParseExact(start, "yyyy-M-d HH:mm", CultureInfo.InvariantCulture);
                            var endTime = DateTime.ParseExact(end, "yyyy-M-d HH:mm", CultureInfo.InvariantCulture);

                            startTime = startTime.AddYears(DateTime.Now.Year - startTime.Year);
                            endTime = endTime.AddYears(DateTime.Now.Year - endTime.Year);

                            var isNow = startTime <= DateTime.Now && DateTime.Now <= endTime;

                            if (_trace)
                                FConsole.WriteLine($"[{nameof(CqActionProcessor)}] [{action.id}] NTT: {ntt.Id}|{ntt.NetId} -> {taskType} -> {startTime} to {endTime} -> {(isNow ? "Success" : "Fail")} -> {(isNow ? action.id_next : action.id_nextfail)}");

                            return isNow ? action.id_next : action.id_nextfail;
                        }
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