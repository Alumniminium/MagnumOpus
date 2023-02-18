using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
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
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, value, MsgUserAttribType.Strength);
                entity.NetSync(ref packet, true);
            }
        }
        public ushort Agility
        {
            get => agility; 
            set
            {
                agility = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, value, MsgUserAttribType.Agility);
                entity.NetSync(ref packet, true);
            }
        }
        public ushort Vitality
        {
            get => vitality; 
            set
            {
                vitality = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, value, MsgUserAttribType.Vitality);
                entity.NetSync(ref packet, true);
            }
        }
        public ushort Spirit
        {
            get => spirit; 
            set
            {
                spirit = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, value, MsgUserAttribType.Spirit);
                entity.NetSync(ref packet, true);
            }
        }
        public ushort Statpoints
        {
            get => statpoints; 
            set
            {
                statpoints = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, value, MsgUserAttribType.StatPoints);
                entity.NetSync(ref packet, true);
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