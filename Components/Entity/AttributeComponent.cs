using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct AttributeComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        private ushort strength;
        private ushort agility;
        private ushort vitality;
        private ushort spirit;
        private ushort statpoints;

        public ushort Strength
        {
            get => strength;
            set
            {
                strength = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, value, MsgUserAttribType.Strength);
                ntt.NetSync(ref packet, true);
            }
        }
        public ushort Agility
        {
            get => agility;
            set
            {
                agility = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, value, MsgUserAttribType.Agility);
                ntt.NetSync(ref packet, true);
            }
        }
        public ushort Vitality
        {
            get => vitality;
            set
            {
                vitality = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, value, MsgUserAttribType.Vitality);
                ntt.NetSync(ref packet, true);
            }
        }
        public ushort Spirit
        {
            get => spirit;
            set
            {
                spirit = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, value, MsgUserAttribType.Spirit);
                ntt.NetSync(ref packet, true);
            }
        }
        public ushort Statpoints
        {
            get => statpoints;
            set
            {
                statpoints = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, value, MsgUserAttribType.StatPoints);
                ntt.NetSync(ref packet, true);
            }
        }

        public AttributeComponent(int entityId)
        {
            EntityId = entityId;
            Strength = 0;
            Agility = 0;
            Vitality = 0;
            Spirit = 0;
            Statpoints = 0;
        }
        public override int GetHashCode() => EntityId;
    }
}