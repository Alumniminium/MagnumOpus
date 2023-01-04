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
                Param2 = uniqueId,
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
                        FConsole.WriteLine($"[GAME] Revive: {ntt.NetId}");
                        var rev = new ReviveComponent(ntt.Id, 1);
                        ntt.Add(ref rev);
                        break;
                    }
                case MsgActionType.SendLocation:
                    {
                        FConsole.WriteLine($"[GAME] SendLocation: {ntt.NetId} -> {msg.X}, {msg.Y}");
                        ref var pos = ref ntt.Get<PositionComponent>();

                        var reply = Create(ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);
                        ntt.NetSync(ref reply);
                        PixelWorld.Players.Add(ntt);
                        break;
                    }
                case MsgActionType.LeaveBooth:
                case MsgActionType.ConfirmGuild:
                case MsgActionType.SendItems:
                case MsgActionType.SendAssociates:
                case MsgActionType.SendProficiencies:
                    {
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.NetId}");
                        ntt.NetSync(memory[..msg.Size]);
                        break;
                    }
                case MsgActionType.SendSpells:
                    {
                        ref readonly var sbc = ref ntt.Get<SpellBookComponent>();
                        foreach(var spell in sbc.Spells)
                        {
                            var reply = MsgSkill.Create(spell.Key, spell.Value.exp, spell.Value.lvl);
                            ntt.NetSync(ref reply);
                        }
                        ntt.NetSync(memory[..msg.Size]);
                        break;
                    }
                    case MsgActionType.ChangeFace:
                    {
                        ref var bdy = ref ntt.Get<BodyComponent>();
                        bdy.FaceId = (ushort)msg.Param;
                        ntt.NetSync(memory[..msg.Size], true);
                        break;
                    }
                case MsgActionType.ChangeFacing:
                    {
                        FConsole.WriteLine($"[GAME] ChangeFacing: {ntt.NetId} -> {msg.Direction}");
                        var dir = new DirectionComponent(ntt.Id, msg.Direction);
                        ntt.Add(ref dir);
                        break;
                    }
                case MsgActionType.ChangeAction:
                    {
                        FConsole.WriteLine($"[GAME] ChangeAction: {ntt.NetId} -> {msg.Param}");
                        var emo = new EmoteComponent(ntt.Id, (Emote)msg.Param);
                        ntt.Add(ref emo);
                        break;
                    }
                case MsgActionType.Jump:
                    {
                        FConsole.WriteLine($"[GAME] Jump: {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        var jmp = new JumpComponent(ntt.Id, msg.JumpX, msg.JumpY);
                        var dir = new DirectionComponent(ntt.Id, msg.Direction);
                        ntt.Add(ref jmp);
                        ntt.Add(ref dir);
                        break;
                    }
                case MsgActionType.EnterPortalChangeMap:
                    {
                        FConsole.WriteLine($"[GAME] EnterPortalChangeMap: {ntt.NetId} -> {msg.Param}");
                        var tpc = new PortalComponent(ntt.Id, msg.X, msg.Y);
                        ntt.Add(ref tpc);
                        break;
                    }
                case MsgActionType.QueryEntity:
                    {
                        FConsole.WriteLine($"[GAME] QueryEntity: {ntt.NetId} -> {msg.Param}");
                        ref readonly var ent = ref PixelWorld.GetEntityByNetId(msg.Param);
                        NetworkHelper.FullSync(in ntt, in ent);
                        break;
                    }
                case MsgActionType.TeleportReply:
                    {
                        FConsole.WriteLine($"[GAME] TeleportReply: {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        ref var pos = ref ntt.Get<PositionComponent>();
                        pos.Position = new Vector2(msg.JumpX, msg.JumpY);
                        pos.ChangedTick = PixelWorld.Tick;
                        ntt.NetSync(memory[..msg.Size]);
                        break;
                    }
                case MsgActionType.GuardJump:
                    {
                        FConsole.WriteLine($"[GAME] GuardJump: {ntt.NetId} -> {msg.JumpX}, {msg.JumpY}");
                        ntt.NetSync(memory[..msg.Size]);
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