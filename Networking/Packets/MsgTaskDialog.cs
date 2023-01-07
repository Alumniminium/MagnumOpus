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

        public static MsgTaskDialog Create(in PixelEntity target, byte optionId, MsgTaskDialogAction action, string text = "")
        {
            ref readonly var hed = ref target.Get<HeadComponent>();
            var packet = new MsgTaskDialog
            {
                Size = (ushort)(14 + text.Length),
                Id = 2032,
                UniqeId = target.NetId,
                Avatar = hed.FaceId,
                OptionId = optionId,
                Action = action
            };

            packet.Unknown[0] = (byte)1;
            packet.Unknown[1] = (byte)text.Length;
            for (int i = 0; i < text.Length; i++)
                packet.Unknown[2 + i] = (byte)text[i];

            return packet;
        }

        [PacketHandler(PacketId.MsgDialog)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msgTaskDialog = Co2Packet.Deserialze<MsgTaskDialog>(in memory);
            var npc = PixelWorld.GetEntityByNetId(msgTaskDialog.UniqeId);

            FConsole.WriteLine($"MsgTaskDialog: Npc {npc.NetId}, action: {msgTaskDialog.Action}, Option: {msgTaskDialog.OptionId}");
            var cq_npc = CqProcessor.GetNpc(npc.NetId);

            if (cq_npc == null)
                return;

            FConsole.WriteLine($"Task 0: {cq_npc.task0}");

            var cq_task = CqProcessor.GetTask(cq_npc.task0);
            if (cq_task == null)
                return;

            FConsole.WriteLine($"Task: {cq_task.id}, Next: {cq_task.id_next}, Fail: {cq_task.id_nextfail}");

            cq_action action;
            var nextId = cq_task.id_next;
            var taskComponent = new CqTaskComponent(ntt.Id, npc.Id);
            ntt.Add(ref taskComponent);
            do
            {
                action = CqProcessor.GetAction(nextId);

                if (action == null)
                    break;

                nextId = CqActionProcessor.Process(in ntt, action);
                FConsole.WriteLine($"Type: {action.type}, Data: {action.param.Trim()}, Next: {action.id_next}, Fail: {action.id_nextfail}");
            }
            while (nextId != 0); // copilot is a bit 
        }

        [PacketHandler(PacketId.MsgDialog2)]
        public static void Process2(PixelEntity ntt, Memory<byte> memory)
        {
            if (!ntt.Has<CqTaskComponent>())
                return;

            var msgTaskDialog = Co2Packet.Deserialze<MsgTaskDialog>(in memory);

            if (msgTaskDialog.OptionId == 255 || msgTaskDialog.OptionId == 0)
            {
                ntt.Remove<CqTaskComponent>();
                return;
            }

            var npc = PixelWorld.GetEntityByNetId(msgTaskDialog.UniqeId);
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
            do
            {
                var action = CqProcessor.GetAction(nextId);
                if (action == null)
                    break;

                // FConsole.WriteLine($"Type: {task.type}, Data: {task.param.Trim()}, Next: {task.id_next}, Fail: {task.id_nextfail}");
                
                nextId = CqActionProcessor.Process(in ntt, action);
                task = CqProcessor.GetTask(nextId);
            }
            while (nextId != 0); // copilot is a bit 
        }
    }
}