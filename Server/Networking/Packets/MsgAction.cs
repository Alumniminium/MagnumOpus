using System.Numerics;
using System.Runtime.InteropServices;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Networking.Packets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct MsgAction
{
    [FieldOffset(0)]
    public MsgHeader Header;
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

    public static MsgAction Create(int uniqueId, int param, ushort x, ushort y, Direction direction, MsgActionType type)
    {
        MsgAction msgP = new()
        {
            Header = new MsgHeader { Size = (ushort)sizeof(MsgAction), Id = PacketId.MsgAction },
            Timestamp = (int)NttWorld.Tick,
            UniqueId = uniqueId,
            Param = param,
            X = x,
            Y = y,
            Direction = direction,
            Type = type
        };
        return msgP;
    }
    public static MsgAction CreateJump(in NTT ntt, in JumpComponent jmp)
    {
        MsgAction msgP = new()
        {
            Header = new MsgHeader { Size = (ushort)sizeof(MsgAction), Id = PacketId.MsgAction },
            Timestamp = (int)NttWorld.Tick,
            UniqueId = ntt.Id,
            JumpX = (ushort)jmp.Position.X,
            JumpY = (ushort)jmp.Position.Y,
            Direction = 0,
            Type = MsgActionType.Jump
        };
        return msgP;
    }

    public static MsgAction RemoveEntity(int uniqueId)
    {
        MsgAction msgP = new()
        {
            Header = new MsgHeader { Size = (ushort)sizeof(MsgAction), Id = PacketId.MsgAction },
            Timestamp = (int)NttWorld.Tick,
            UniqueId = uniqueId,
            Param = uniqueId,
            Type = MsgActionType.RemoveEntity
        };
        return msgP;
    }

    [PacketHandler(PacketId.MsgAction)]
    public static void Process(NTT ntt, Memory<byte> memory)
    {
        var _trace = true;
        var msg = Co2Packet.Deserialize<MsgAction>(memory.Span);

        switch (msg.Type)
        {
            case MsgActionType.Revive:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id}");
                    var rev = new ReviveComponent(1);
                    ntt.Set(ref rev);
                    break;
                }
            case MsgActionType.QueryLocation:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.X}, {msg.Y}");

                    ref var pos = ref ntt.Get<PositionComponent>();
                    var reply = Create(ntt.Id, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.QueryLocation);
                    ntt.NetSync(ref reply);
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Outgoing {msg.Type}: {ntt.Id} -> {reply.X}, {reply.Y}");

                    NttWorld.Players.Add(ntt);
                    ntt.Set<ViewportUpdateTagComponent>();
                    break;
                }
            case MsgActionType.LeaveBooth:
            case MsgActionType.QueryGuild:
            case MsgActionType.QueryFriends:
            case MsgActionType.QuerySkills:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id}");

                    ntt.NetSync(ref msg);
                    break;
                }
            case MsgActionType.QueryItems:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id}");

                    ref var inv = ref ntt.Get<InventoryComponent>();

                    InventoryHelper.SortById(ntt, ref inv, netSync: true);
                    NetworkHelper.SyncEquipment(ntt);

                    ntt.NetSync(ref msg);
                    break;
                }
            case MsgActionType.QuerySpells:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id}");

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
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id}");

                    ref var head = ref ntt.Get<HeadComponent>();
                    head.FaceId = (ushort)msg.Param;
                    ntt.NetSync(ref msg, true);
                    break;
                }
            case MsgActionType.UpdateFacing:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.Direction}");

                    ref var pos = ref ntt.Get<PositionComponent>();
                    pos.Direction = msg.Direction;
                    ntt.NetSync(ref msg, true);
                    break;
                }
            case MsgActionType.UpdateEmote:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.Param}");

                    var emo = new EmoteComponent((Emote)msg.Param);
                    ntt.Set(ref emo);
                    break;
                }
            case MsgActionType.Jump:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");

                    var jmp = new JumpComponent(msg.JumpX, msg.JumpY);
                    var emo = new EmoteComponent(Emote.Stand);
                    ntt.Set(ref jmp);
                    ntt.Set(ref emo);
                    break;
                }
            case MsgActionType.EnterPortalChangeMap:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.Param}");

                    var tpc = new PortalComponent(msg.X, msg.Y);
                    ntt.Set(ref tpc);
                    break;
                }
            case MsgActionType.QueryEntity:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.Param}");

                    ref readonly var ent = ref NttWorld.GetEntity(msg.Param);
                    if (ent.Id != 0)
                        NetworkHelper.FullSync(in ntt, in ent);
                    else
                        ntt.NetSync(ref msg);

                    ref readonly var vwp = ref ntt.Get<ViewportComponent>();
                    vwp.EntitiesVisible.Add(ent);
                    break;
                }
            case MsgActionType.QueryTeamMember:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.Param}");

                    ref readonly var ent = ref NttWorld.GetEntity(msg.Param);
                    if (ent.Id != 0)
                    {
                        ref readonly var team = ref ent.Get<TeamComponent>();
                        ref readonly var pos = ref ent.Get<PositionComponent>();

                        var leaderPos = Create(ntt.Id, ntt.Id, (ushort)pos.Position.X, (ushort)pos.Position.Y, 0, MsgActionType.QueryTeamMember);
                        ntt.NetSync(ref leaderPos);
                    }
                    break;
                }
            case MsgActionType.TeleportReply:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type}: {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");

                    ref var pos = ref ntt.Get<PositionComponent>();
                    pos.Position = new Vector2(msg.JumpX, msg.JumpY);
                    ntt.NetSync(ref msg);
                    ntt.Set<ViewportUpdateTagComponent>();
                    break;
                }
            case MsgActionType.GuardJump:
                {
                    if (_trace)
                        FConsole.WriteLine($"[GAME] Incomming {msg.Type} : {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");

                    ntt.NetSync(ref msg);
                    break;
                }
            default:
                {
                    if (!_trace)
                        break;

                    FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                    FConsole.WriteLine(memory.Dump());
                    break;
                }
        }
    }
}