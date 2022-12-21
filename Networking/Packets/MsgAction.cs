using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;

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

        public static Memory<byte> Create(int timestamp, int uniqueId, int param, ushort x, ushort y, Direction direction, MsgActionType type)
        {
            MsgAction msgP = new()
            {
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = timestamp,
                UniqueId = uniqueId,
                Param = param,
                X = x,
                Y = y,
                Direction = direction,
                Type = type
            };
            return msgP;
        }

        [PacketHandler(PacketId.MsgAction)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = (MsgAction)memory;
            
            switch (msg.Type)
            {
                case MsgActionType.SendLocation:
                    {
                        if (!ntt.Has<PositionComponent>())
                        {
                            var pc = new PositionComponent(ntt.Id, new Vector2(438, 377), 1002);
                            ntt.Add(ref pc);
                        }
                        ref var pos = ref ntt.Get<PositionComponent>();
                        var reply = MsgAction.Create(0, ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);
                        ntt.NetSync(in reply);
                        PixelWorld.Players.Add(ntt);
                        break;
                    }
                case MsgActionType.SendItems:
                case MsgActionType.SendAssociates:
                case MsgActionType.SendProficiencies:
                case MsgActionType.SendSpells:
                    {
                        var reply = MsgAction.Create(0, ntt.NetId, 0, 0, 0, 0, msg.Type);
                        ntt.NetSync(in reply);
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
                        ntt.NetSync(memory[0..sizeof(MsgAction)]);
                        break;
                    }
                case MsgActionType.ChangeMap:
                    {

                        break;
                    }
                case MsgActionType.Teleport:
                    break;
                case MsgActionType.LevelUp:
                    break;
                case MsgActionType.XpClear:
                    break;
                case MsgActionType.Revive:
                    break;
                case MsgActionType.DelRole:
                    break;
                case MsgActionType.SetKillMode:
                    break;
                case MsgActionType.ConfirmGuild:
                    break;
                case MsgActionType.Mine:
                    break;
                case MsgActionType.TeamMemberPos:
                    break;
                case MsgActionType.QueryEntity:
                    break;
                case MsgActionType.AbortMagic:
                    break;
                case MsgActionType.MapARGB:
                    break;
                case MsgActionType.MapStatus:
                    break;
                case MsgActionType.QueryTeamMember:
                    break;
                case MsgActionType.Kickback:
                    break;
                case MsgActionType.DropMagic:
                    break;
                case MsgActionType.DropSkill:
                    break;
                case MsgActionType.CreateBooth:
                    break;
                case MsgActionType.SuspendBooth:
                    break;
                case MsgActionType.ResumeBooth:
                    break;
                case MsgActionType.LeaveBooth:
                    break;
                case MsgActionType.PostCommand:
                    break;
                case MsgActionType.QueryEquipment:
                    break;
                case MsgActionType.AbortTransform:
                    break;
                case MsgActionType.EndFly:
                    break;
                case MsgActionType.GetMoney:
                    break;
                case MsgActionType.QueryEnemy:
                    break;
                case MsgActionType.OpenDialog:
                    break;
                case MsgActionType.GuardJump:
                    break;
                case MsgActionType.SpawnEffect:
                    break;
                case MsgActionType.RemoveEntity:
                    break;
                case MsgActionType.TeleportReply:
                    break;
                case MsgActionType.DeathConfirmation:
                    break;
                case MsgActionType.QueryAssociateInfo:
                    break;
                case MsgActionType.ChangeFace:
                    break;
                case MsgActionType.ItemsDetained:
                    break;
                case MsgActionType.NinjaStep:
                    break;
                case MsgActionType.HideInterface:
                    break;
                case MsgActionType.OpenUpgrade:
                    break;
                case MsgActionType.AwayFromKeyboard:
                    break;
                case MsgActionType.PathFinding:
                    break;
                case MsgActionType.DragonBallDropped:
                    break;
                case MsgActionType.TableState:
                    break;
                case MsgActionType.TablePot:
                    break;
                case MsgActionType.TablePlayerCount:
                    break;
                case MsgActionType.QueryFriendEquip:
                    break;
                case MsgActionType.QueryStatInfo:
                    break;
                default:
                    {
                        FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                        var reply = Create(msg.Timestamp, ntt.NetId, msg.Param, msg.X, msg.Y, msg.Direction, msg.Type);
                        ntt.NetSync(in reply);
                        break;
                    }
            }
        }

        public static implicit operator Memory<byte>(MsgAction msg)
        {
            var buffer = new byte[msg.Size];
            fixed(byte* ptr = buffer)
                *(MsgAction*)ptr = msg;
            return buffer;
        }
        public static implicit operator MsgAction(in Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(MsgAction*)p;
        }
    }
}