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
        public static void Process(in PixelEntity ntt, cq_action action)
        {
            var taskType = (TaskActionType)action.type;

            switch (taskType)
            {
                case TaskActionType.ACTION_MENUTEXT:
                    {
                        ref readonly var tac = ref ntt.Get<TaskComponent>();
                        var textPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Text, action.param.Replace("~", " "));
                        var textMem = Co2Packet.Serialize(ref textPacket, textPacket.Size);
                        ntt.NetSync(textMem);
                        break;
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
                        break;
                    }
                case TaskActionType.ACTION_MENUPIC:
                    {
                        ref readonly var tac = ref ntt.Get<TaskComponent>();
                        var facePacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Picture);
                        var faceId = byte.Parse(action.param.Trim().Split(' ')[2]);
                        facePacket.Avatar = faceId;
                        var faceMem = Co2Packet.Serialize(ref facePacket, facePacket.Size);
                        ntt.NetSync(faceMem);
                        break;
                    }
                case TaskActionType.ACTION_MENUCREATE:
                    {
                        ref readonly var tac = ref ntt.Get<TaskComponent>();
                        var showPacket = MsgTaskDialog.Create(in tac.Npc, 0, MsgTaskDialogAction.Create);
                        var showMem = Co2Packet.Serialize(ref showPacket, showPacket.Size);
                        ntt.NetSync(showMem);
                        break;
                    }
                default:
                    FConsole.WriteLine($"[FAIL] Unknown task type: {taskType}");
                    FConsole.WriteLine($"| - Task:     {action.id}");
                    FConsole.WriteLine($"| - Param:    {action.param}");
                    FConsole.WriteLine($"| - Data:     {action.data}");
                    FConsole.WriteLine($"| - Next:     {action.id_next}");
                    FConsole.WriteLine($"| - Fail:     {action.id_nextfail}");

                    break;
            }
        }
    }
}