using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct HealthComponent
    {
        public NTT NTT;
        private int health;
        private int maxHealth;

        public int Health
        {
            get => health; set
            {
                health = value;
                var packet = MsgUserAttrib.Create(NTT.Id, (uint)health, MsgUserAttribType.Health);
                NTT.NetSync(ref packet, true);
            }
        }
        public int MaxHealth
        {
            get => maxHealth; set
            {
                maxHealth = value;
                var packet = MsgUserAttrib.Create(NTT.Id, (uint)maxHealth, MsgUserAttribType.MaxHealth);
                NTT.NetSync(ref packet, true);
            }
        }

        public HealthComponent(in NTT ntt, ushort health, ushort maxHealth)
        {
            NTT = ntt;
            Health = health;
            MaxHealth = maxHealth;
        }
        public override int GetHashCode() => NTT;
    }
}