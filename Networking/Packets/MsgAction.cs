using System.Numerics;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;
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
                case MsgActionType.SendLocation:
                    {
                        if (!ntt.Has<PositionComponent>())
                        {
                            var pc = new PositionComponent(ntt.Id, new Vector2(230, 192), 1002);
                            ntt.Add(ref pc);
                        }
                        ref var pos = ref ntt.Get<PositionComponent>();
                        var reply = Create(ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);
                        ntt.NetSync(ref reply);
                        PixelWorld.Players.Add(ntt);
                        break;
                    }
                case MsgActionType.ConfirmGuild:
                case MsgActionType.SendItems:
                case MsgActionType.SendAssociates:
                case MsgActionType.SendProficiencies:
                case MsgActionType.SendSpells:
                    {
                        var reply = Create(ntt.NetId, 0, 0, 0, 0, msg.Type);
                        ntt.NetSync(ref reply);
                        break;
                    }
                case MsgActionType.ChangeFacing:
                    {
                        var dir = new DirectionComponent(ntt.Id, msg.Direction);
                        ntt.Add(ref dir);
                        break;
                    }
                case MsgActionType.ChangeAction:
                    {
                        var emo = new EmoteComponent(ntt.Id, (Emote)msg.Param);
                        ntt.Add(ref emo);
                        break;
                    }
                case MsgActionType.Jump:
                    {
                        var jmp = new JumpComponent(ntt.Id, msg.JumpX, msg.JumpY);
                        ntt.Add(ref jmp);
                        break;
                    }
                case MsgActionType.EnterPortalChangeMap:
                {
                    var tpc = new PortalComponent(ntt.Id, msg.X, msg.Y);
                    ntt.Add(ref tpc);
                    break;
                }
                default:
                    {
                        FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                        FConsole.WriteLine(memory.Dump());
                        var reply = Create(ntt.NetId, msg.Param, msg.X, msg.Y, msg.Direction, msg.Type);
                        ntt.NetSync(ref reply);
                        break;
                    }
            }
        }
    }
}