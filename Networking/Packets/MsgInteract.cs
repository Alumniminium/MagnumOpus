using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components.Attack;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Cryptography;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgInteract
    {
        public ushort Size;
        public ushort Id;
        public int Timestamp;
        public int AttackerUniqueId;
        public int TargetUniqueId;
        public ushort X;
        public ushort Y;
        public MsgInteractType Type;
        public int Value;
        public int Value2;

        public static MsgInteract Create(in NTT source, in NTT target, MsgInteractType type, int value)
        {
            ref readonly var bdy = ref target.Get<BodyComponent>();
            ref readonly var pos = ref target.Get<PositionComponent>();

            var msg = new MsgInteract
            {
                Size = (ushort)sizeof(MsgInteract),
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = source.Id,
                TargetUniqueId = target.Id,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Type = type,
                Value = value,
            };
            return msg;
        }
        public static MsgInteract Create(int attackerUniqueId, int targetUniqueId, ushort targetX, ushort targetY, MsgInteractType type, int value)
        {
            var msg = new MsgInteract
            {
                Size = (ushort)sizeof(MsgInteract),
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = attackerUniqueId,
                TargetUniqueId = targetUniqueId,
                X = targetX,
                Y = targetY,
                Type = type,
                Value = value,
            };
            return msg;
        }
        public static MsgInteract Die(in NTT attacker, NTT target)
        {
            ref readonly var bdy = ref target.Get<PositionComponent>();
            var msg = new MsgInteract
            {
                Size = 32,
                Id = 1022,
                Timestamp = Environment.TickCount,
                AttackerUniqueId = attacker.Id,
                TargetUniqueId = target.Id,
                X = (ushort)bdy.Position.X,
                Y = (ushort)bdy.Position.Y,
                Type = MsgInteractType.Death,
                Value = 0,
            };

            return msg;
        }

        [PacketHandler(PacketId.MsgInteract)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgInteract>(memory);

            switch (msg.Type)
            {
                case MsgInteractType.Archer:
                case MsgInteractType.Physical:
                    {
                        if (ntt.Id != msg.AttackerUniqueId)
                        {
                            FConsole.WriteLine($"[MsgInteract] HAX! {ntt.Id} != {msg.AttackerUniqueId}");
                            return;
                        }

                        var target = NttWorld.GetEntity(msg.TargetUniqueId);

                        // TODO: check if target not invalid

                        var atk = new AttackComponent(in target, msg.Type);
                        ntt.Set(ref atk);
                        break;
                    }
                case MsgInteractType.Magic:
                    {
                        var (skillId, targetId, x, y) = SpellCrypto.DecryptSkill(in ntt, ref msg);

                        if (ntt.Id != msg.AttackerUniqueId)
                        {
                            FConsole.WriteLine($"[MsgInteract] HAX! {ntt.Id} != {msg.AttackerUniqueId}");
                            return;
                        }

                        var mAtk = new MagicAttackRequestComponent(skillId, targetId, x, y, NttWorld.TargetTps);
                        ntt.Set(ref mAtk);

                        // var target = PixelWorld.GetEntityByNetId((int)targetId);

                        // var atkMsg = MsgMagicEffect.Create(in ntt, in target, 10, skillId, 0);
                        // ntt.NetSync(ref atkMsg, true);
                        break;
                    }

                case MsgInteractType.None:
                    break;
                case MsgInteractType.RequestMarriage:
                    break;
                case MsgInteractType.AcceptMarriage:
                    break;
                case MsgInteractType.Death:
                    break;
                case MsgInteractType.MonsterHunter:
                    break;
                default:
                    break;
            }
        }
    }
}