using System.Numerics;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgAction
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int Timestamp;
        [FieldOffset(8)]
        public int UniqueId;
        [FieldOffset(12)]
        public int Param;
        [FieldOffset(12)]
        public ushort JumpX;
        [FieldOffset(14)]
        public ushort JumpY;
        [FieldOffset(16)]
        public int Param2;
        [FieldOffset(16)]
        public ushort X;
        [FieldOffset(18)]
        public ushort Y;
        [FieldOffset(20)]
        public Direction Direction;
        [FieldOffset(22)]
        public MsgActionType Type;

        public static MsgAction RemoveEntity(int uniqueId)
        {
            MsgAction msgP = new()
            {
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = (int)PixelWorld.Tick,
                UniqueId = uniqueId,
                Param = uniqueId,
                Type = MsgActionType.RemoveEntity
            };
            return msgP;
        }
        public static MsgAction Create(int uniqueId, int param, ushort x, ushort y, Direction direction, MsgActionType type)
        {
            MsgAction msgP = new()
            {
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = (int)PixelWorld.Tick,
                UniqueId = uniqueId,
                Param = param,
                X = x,
                Y = y,
                Direction = direction,
                Type = type
            };
            return msgP;
        }
        public static MsgAction CreateJump(in PixelEntity ntt, in JumpComponent jmp)
        {
            MsgAction msgP = new()
            {
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = (int)PixelWorld.Tick,
                UniqueId = ntt.NetId,
                JumpX = (ushort)jmp.Position.X,
                JumpY = (ushort)jmp.Position.Y,
                Direction = 0,
                Type = MsgActionType.Jump
            };
            return msgP;
        }

        [PacketHandler(PacketId.MsgAction)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgAction>(in memory);

            switch (msg.Type)
            {
                case MsgActionType.Revive:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        var rev = new ReviveComponent(ntt.Id, 1);
                        ntt.Set(ref rev);
                        break;
                    }
                case MsgActionType.SendLocation:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.X}, {msg.Y}");
                        ref var pos = ref ntt.Get<PositionComponent>();

                        var reply = Create(ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);
                        ntt.NetSync(ref reply);
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {reply.X}, {reply.Y}");

                        PixelWorld.Players.Add(ntt);
                        break;
                    }
                case MsgActionType.LeaveBooth:
                case MsgActionType.ConfirmGuild:
                case MsgActionType.SendAssociates:
                case MsgActionType.SendProficiencies:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.SendItems:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        ref readonly var inv = ref ntt.Get<InventoryComponent>();

                        foreach (var item in inv.Items)
                        {
                            ref readonly var itemComp = ref item.Get<ItemComponent>();
                            var reply = MsgItem.Create(item.NetId, itemComp.Id, itemComp.Id, 0, MsgItemType.Buy);
                            var reply2 = MsgItemInformation.Create(in item);
                            ntt.NetSync(ref reply);
                            ntt.NetSync(ref reply2);
                        }
                        ntt.NetSync(ref msg);

                        break;
                    }
                case MsgActionType.SendSpells:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        ref readonly var sbc = ref ntt.Get<SpellBookComponent>();
                        foreach (var spell in sbc.Spells)
                        {
                            var reply = MsgSkill.Create(spell.Key, spell.Value.exp, spell.Value.lvl);
                            ntt.NetSync(ref reply);
                        }
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.ChangeFace:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        ref var head = ref ntt.Get<HeadComponent>();
                        head.FaceId = (ushort)msg.Param;
                        ntt.NetSync(ref msg, true);
                        break;
                    }
                case MsgActionType.ChangeFacing:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.Direction}");
                        var dir = new DirectionComponent(ntt.Id, msg.Direction);
                        ntt.Set(ref dir);
                        break;
                    }
                case MsgActionType.ChangeAction:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.Param}");
                        var emo = new EmoteComponent(ntt.Id, (Emote)msg.Param);
                        ntt.Set(ref emo);
                        break;
                    }
                case MsgActionType.Jump:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        var jmp = new JumpComponent(ntt.Id, msg.JumpX, msg.JumpY);
                        var dir = new DirectionComponent(ntt.Id, msg.Direction);
                        var emo = new EmoteComponent(ntt.Id, Emote.Stand);
                        ntt.Set(ref jmp);
                        ntt.Set(ref dir);
                        ntt.Set(ref emo);
                        break;
                    }
                case MsgActionType.EnterPortalChangeMap:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.Param}");
                        var tpc = new PortalComponent(ntt.Id, msg.X, msg.Y);
                        ntt.Set(ref tpc);
                        break;
                    }
                case MsgActionType.QueryEntity:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.Param}");
                        ref readonly var ent = ref PixelWorld.GetEntityByNetId(msg.Param);
                        if (ent.Id != 0)
                            NetworkHelper.FullSync(in ntt, in ent);
                        else
                            ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.TeleportReply:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        ref var pos = ref ntt.Get<PositionComponent>();
                        pos.Position = new Vector2(msg.JumpX, msg.JumpY);
                        pos.ChangedTick = PixelWorld.Tick;
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.GuardJump:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type} : {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        ntt.NetSync(ref msg);
                        break;
                    }
                default:
                    {
                        FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                        FConsole.WriteLine(memory.Dump());
                        break;
                    }
            }
        }
    }
}