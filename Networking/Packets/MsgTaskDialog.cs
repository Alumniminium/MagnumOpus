using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTaskDialog
    {
        public ushort Size;
        public ushort Id;
        public int UniqeId;
        public ushort Avatar;
        public byte OptionId;
        public MsgTaskDialogAction Action;
        public fixed byte Unknown[512];

        public static MsgTaskDialog Create(in NTT target, byte optionId, MsgTaskDialogAction action, string text = "")
        {
            ref readonly var hed = ref target.Get<HeadComponent>();
            var packet = new MsgTaskDialog
            {
                Size = (ushort)(14 + text.Length),
                Id = 2032,
                UniqeId = target.Id,
                Avatar = hed.FaceId,
                OptionId = optionId,
                Action = action
            };

            packet.Unknown[0] = 1;
            packet.Unknown[1] = (byte)text.Length;
            for (var i = 0; i < text.Length; i++)
                packet.Unknown[2 + i] = (byte)text[i];

            return packet;
        }

        [PacketHandler(PacketId.MsgDialog)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msgTaskDialog = Co2Packet.Deserialize<MsgTaskDialog>(memory.Span);
            var npc = NttWorld.GetEntity(msgTaskDialog.UniqeId);

            FConsole.WriteLine($"MsgTaskDialog: Npc {npc.Id}, action: {msgTaskDialog.Action}, Option: {msgTaskDialog.OptionId}");
            var cq_npc = CqProcessor.GetNpc(npc.Id);
            ref readonly var task = ref npc.Get<CqActionComponent>();

            if (cq_npc == null)
                return;

            FConsole.WriteLine($"Task 0: {cq_npc.task0}");

            var cq_task = CqProcessor.GetTask(cq_npc.task0);
            if (cq_task == null)
                return;

            FConsole.WriteLine($"Task: {cq_task.id}, Next: {cq_task.id_next}, Fail: {cq_task.id_nextfail}");

            cq_action? action;
            var nextId = cq_task.id_next;
            var taskComponent = new CqTaskComponent(npc.Id);
            ntt.Set(ref taskComponent);
            do
            {
                action = CqProcessor.GetAction(nextId);

                if (action == null)
                    break;

                nextId = CqActionProcessor.Process(in ntt, in npc, action);
                FConsole.WriteLine($"Type: {action.type}, Data: {action.param.Trim()}, Next: {action.id_next}, Fail: {action.id_nextfail}");
            }
            while (nextId != 0); // copilot is a bit 
        }

        [PacketHandler(PacketId.MsgDialog2)]
        public static void Process2(NTT ntt, Memory<byte> memory)
        {
            if (!ntt.Has<CqTaskComponent>())
                return;

            var msgTaskDialog = Co2Packet.Deserialize<MsgTaskDialog>(memory.Span);

            if (msgTaskDialog.OptionId is 255 or 0)
            {
                ntt.Remove<CqTaskComponent>();
                return;
            }

            var npc = NttWorld.GetEntity(msgTaskDialog.UniqeId);
            using var ctx = new SquigglyContext();

            ref readonly var taskComponent = ref ntt.Get<CqTaskComponent>();
            var option = taskComponent.Options[msgTaskDialog.OptionId];

            if (option == -1)
            {
                ntt.Remove<CqTaskComponent>();
                return;
            }
            var task = CqProcessor.GetTask(option);

            if (task == null)
            {
                ntt.Remove<CqTaskComponent>();
                return;
            }


            var nextId = task.id_next;
            for (var i = 0; i < 32; i++)
            {
                var action = CqProcessor.GetAction(nextId);
                if (action == null || action.id == 0)
                    break;

                // FConsole.WriteLine($"Type: {task.type}, Data: {task.param.Trim()}, Next: {task.id_next}, Fail: {task.id_nextfail}");

                nextId = CqActionProcessor.Process(in ntt, in npc, action);
                task = CqProcessor.GetTask(nextId);
            }
        }
    }
}