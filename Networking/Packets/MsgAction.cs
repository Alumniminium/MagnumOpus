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
                Timestamp = (int)NttWorld.Tick,
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
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = (int)NttWorld.Tick,
                UniqueId = ntt.Id,
                JumpX = (ushort)jmp.Position.X,
                JumpY = (ushort)jmp.Position.Y,
                Direction = 0,
                Type = MsgActionType.Jump
            };
            return msgP;
        }

        [PacketHandler(PacketId.MsgAction)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var _trace = false;
            var msg = Co2Packet.Deserialze<MsgAction>(in memory);

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
                case MsgActionType.SendLocation:
                    {
                        if (_trace)
                            FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.X}, {msg.Y}");

                        ref var pos = ref ntt.Get<PositionComponent>();
                        ref var vwp = ref ntt.Get<ViewportComponent>();
                        vwp.EntitiesVisible.Clear();
                        vwp.EntitiesVisibleLast.Clear();
                        vwp.Dirty = true;
                        pos.ChangedTick = NttWorld.Tick;
                        var reply = Create(ntt.Id, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);
                        ntt.NetSync(ref reply);
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {reply.X}, {reply.Y}");

                        NttWorld.Players.Add(ntt);
                        break;
                    }
                case MsgActionType.LeaveBooth:
                case MsgActionType.ConfirmGuild:
                case MsgActionType.SendAssociates:
                case MsgActionType.SendProficiencies:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id}");
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.SendItems:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id}");
                        ref var inv = ref ntt.Get<InventoryComponent>();
                        ref var eq = ref ntt.Get<EquipmentComponent>();

                        InventoryHelper.SortById(in ntt, ref inv, netSync: true);
                        ntt.NetSync(ref msg);

                        var head = MsgItemInformation.Create(eq.Head, MsgItemInfoAction.AddItem, MsgItemPosition.Head);
                        var garm = MsgItemInformation.Create(eq.Garment, MsgItemInfoAction.AddItem, MsgItemPosition.Garment);
                        var bottle = MsgItemInformation.Create(eq.Bottle, MsgItemInfoAction.AddItem, MsgItemPosition.Bottle);
                        var neck = MsgItemInformation.Create(eq.Necklace, MsgItemInfoAction.AddItem, MsgItemPosition.Necklace);
                        var ring = MsgItemInformation.Create(eq.Ring, MsgItemInfoAction.AddItem, MsgItemPosition.Ring);
                        var chest = MsgItemInformation.Create(eq.Armor, MsgItemInfoAction.AddItem, MsgItemPosition.Armor);
                        var weaponR = MsgItemInformation.Create(eq.RightWeapon, MsgItemInfoAction.AddItem, MsgItemPosition.RightWeapon);
                        var weaponL = MsgItemInformation.Create(eq.LeftWeapon, MsgItemInfoAction.AddItem, MsgItemPosition.LeftWeapon);
                        var boots = MsgItemInformation.Create(eq.Boots, MsgItemInfoAction.AddItem, MsgItemPosition.Boots);

                        ntt.NetSync(ref head, false);
                        ntt.NetSync(ref garm, false);
                        ntt.NetSync(ref bottle, false);
                        ntt.NetSync(ref neck, false);
                        ntt.NetSync(ref ring, false);
                        ntt.NetSync(ref chest, false);
                        ntt.NetSync(ref weaponR, false);
                        ntt.NetSync(ref weaponL, false);
                        ntt.NetSync(ref boots, false);
                        break;
                    }
                case MsgActionType.SendSpells:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id}");
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
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id}");
                        ref var head = ref ntt.Get<HeadComponent>();
                        head.FaceId = (ushort)msg.Param;
                        ntt.NetSync(ref msg, true);
                        break;
                    }
                case MsgActionType.ChangeFacing:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.Direction}");
                        ref var bdy = ref ntt.Get<BodyComponent>();
                        bdy.Direction = msg.Direction;
                        ntt.NetSync(ref msg, true);
                        break;
                    }
                case MsgActionType.ChangeAction:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.Param}");
                        var emo = new EmoteComponent((Emote)msg.Param);
                        ntt.Set(ref emo);
                        break;
                    }
                case MsgActionType.Jump:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");
                        var jmp = new JumpComponent(msg.JumpX, msg.JumpY);
                        var emo = new EmoteComponent(Emote.Stand);
                        ntt.Set(ref jmp);
                        ntt.Set(ref emo);
                        break;
                    }
                case MsgActionType.EnterPortalChangeMap:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.Param}");
                        var tpc = new PortalComponent(msg.X, msg.Y);
                        ntt.Set(ref tpc);
                        break;
                    }
                case MsgActionType.QueryEntity:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.Param}");
                        ref readonly var ent = ref NttWorld.GetEntity(msg.Param);
                        if (ent.Id != 0)
                            NetworkHelper.FullSync(in ntt, in ent);
                        else
                            ntt.NetSync(ref msg);

                        ref readonly var vwp = ref ntt.Get<ViewportComponent>();
                        vwp.EntitiesVisible.TryAdd(ent.Id, ent);
                        break;
                    }
                case MsgActionType.QueryTeamMember:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.Param}");
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
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type}: {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");
                        ref var pos = ref ntt.Get<PositionComponent>();
                        pos.Position = new Vector2(msg.JumpX, msg.JumpY);
                        pos.ChangedTick = NttWorld.Tick;
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgActionType.GuardJump:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] {msg.Type} : {ntt.Id} -> {msg.JumpX}, {msg.JumpY}");
                        ntt.NetSync(ref msg);
                        break;
                    }
                default:
                    {
                        if (_trace) FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                        if (_trace) FConsole.WriteLine(memory.Dump());
                        break;
                    }
            }
        }
    }
}