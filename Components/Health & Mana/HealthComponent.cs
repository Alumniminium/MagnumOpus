using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct HealthComponent
    {
        public readonly int EntityId;
        private int health;
        private int maxHealth;

        public int Health
        {
            get => health; set
            {
                health = value;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, (uint)health, MsgUserAttribType.Health);
                entity.NetSync(ref packet, true);
            }
        }
        public int MaxHealth
        {
            get => maxHealth; set
            {
                maxHealth = value;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, (uint)maxHealth, MsgUserAttribType.MaxHealth);
                entity.NetSync(ref packet, true);
            }
        }

        public HealthComponent(int entityId, ushort health, ushort maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
        }
        public override int GetHashCode() => EntityId;
    }
}