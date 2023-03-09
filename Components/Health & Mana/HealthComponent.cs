using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
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
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, (uint)health, MsgUserAttribType.Health);
                ntt.NetSync(ref packet, true);
            }
        }
        public int MaxHealth
        {
            get => maxHealth; set
            {
                maxHealth = value;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, (uint)maxHealth, MsgUserAttribType.MaxHealth);
                ntt.NetSync(ref packet, true);
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