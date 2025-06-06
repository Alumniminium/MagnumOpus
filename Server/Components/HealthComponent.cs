using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct HealthComponent
    {
        public long ChangedTick = NttWorld.Tick;
        public NTT NTT;
        
        public int Health;
        public int MaxHealth;

        public HealthComponent(in NTT ntt, ushort health, ushort maxHealth)
        {
            NTT = ntt;
            Health = health;
            MaxHealth = maxHealth;
        }

        public void SetHealth(int value)
        {
            if (Health != value)
            {
                Health = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, (uint)Health, MsgUserAttribType.Health);
                NTT.NetSync(ref packet, true);
            }
        }

        public void SetMaxHealth(int value)
        {
            if (MaxHealth != value)
            {
                MaxHealth = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, (uint)MaxHealth, MsgUserAttribType.MaxHealth);
                NTT.NetSync(ref packet, true);
            }
        }
        
        public override int GetHashCode() => NTT;
    }
}