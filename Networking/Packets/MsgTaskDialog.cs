using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
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
            ref readonly var bdy = ref target.Get<BodyComponent>();
            var packet = new MsgTaskDialog
            {
                Size = (ushort)(14 + text.Length),
                Id = 2032,
                UniqeId = target.NetId,
                Avatar = bdy.FaceId,
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
            using var ctx = new SquigglyContext();

            FConsole.WriteLine($"MsgTaskDialog: Npc {npc.NetId}, action: {msgTaskDialog.Action}, Option: {msgTaskDialog.OptionId}");
            var cq_npc = ctx.cq_npc.Find((long)npc.NetId);

            if (cq_npc == null)
                return;

            FConsole.WriteLine($"Task 0: {cq_npc.task0}");

            var cq_task = ctx.cq_task.Find(cq_npc.task0);
            if (cq_task == null)
                return;
            
            FConsole.WriteLine($"Task: {cq_task.id}, Next: {cq_task.id_next}, Fail: {cq_task.id_nextfail}");

            cq_action task;
            if(!cq_task.id_next.HasValue)
                return;

            var nextId = (long)cq_task.id_next;
            
            do
            {
                task = ctx.cq_action.Find((long)nextId);
                
                if (task == null)
                    break;

                nextId = task.id_next;
                FConsole.WriteLine($"Type: {task.type}, Data: {task.param.Trim()}, Next: {task.id_next}, Fail: {task.id_nextfail}");
                if (task.type == 101)
                {
                    var textPacket = Create(npc, 0, MsgTaskDialogAction.Text, task.param.Replace("~", " "));
                    var textMem = Co2Packet.Serialize(ref textPacket, textPacket.Size);
                    ntt.NetSync(textMem);
                }

                if (task.type == 102)
                {
                    var text = task.param.Trim().Split(' ')[0];
                    // var optionId = int.Parse(task.param.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                    var optionPacket = Create(npc, 255, MsgTaskDialogAction.Link, text);
                    var optionMem = Co2Packet.Serialize(ref optionPacket, optionPacket.Size);
                    ntt.NetSync(optionMem);
                }
                if (task.type == 104)
                {
                    var facePacket = Create(npc, 0, MsgTaskDialogAction.Picture);
                    var faceId = byte.Parse(task.param.Trim().Split(' ')[2]);
                    facePacket.Avatar = faceId;
                    var faceMem = Co2Packet.Serialize(ref facePacket, facePacket.Size);
                    ntt.NetSync(faceMem);
                }
                if (task.type == 120)
                {
                    var showPacket = Create(npc, 0, MsgTaskDialogAction.Create);
                    var showMem = Co2Packet.Serialize(ref showPacket, showPacket.Size);
                    ntt.NetSync(showMem);
                }

                if (task.id_next == 0)
                    break;
            }
            while (nextId != 0); // copilot is a bit 
        }
    }
}