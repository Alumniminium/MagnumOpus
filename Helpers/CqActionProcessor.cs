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
                            return true;
                        }
                        return false;
                    }
                    if(BooleanAttrOpts.TryGetValue(op, out var boolFunc))
                        return boolFunc((long)x.Level, targetVal);
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
                        var textPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Text, action.param.Replace("~", " ").Trim());
                        var textMem = Co2Packet.Serialize(ref textPacket, textPacket.Size);
                        ntt.NetSync(textMem);
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

                        var result = AttrVals[attribute](ntt, targetVal, operation);
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
                        if(sbc.Spells.ContainsKey(skillId))
                            return action.id_next;
                        return action.id_nextfail;
                    }
                    if(op == "learn")
                    {
                        sbc.Spells.Add(skillId, (0,0,0));
                        var msg = MsgSkill.Create(skillId, 0,0);
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
                default:
                    FConsole.WriteLine($"[FAIL] Unknown task type: {taskType}");
                    FConsole.WriteLine($"| - Task:     {action.id}");
                    FConsole.WriteLine($"| - Param:    {action.param.Trim()}");
                    FConsole.WriteLine($"| - Data:     {action.data}");
                    FConsole.WriteLine($"| - Next:     {action.id_next}");
                    FConsole.WriteLine($"| - Fail:     {action.id_nextfail}");
                    Console.Beep();
                    return action.id_nextfail;
            }
        }
    }
}