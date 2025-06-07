using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct HealthComponent(in NTT ntt, int health = 100, int maxHealth = 100)
    {
        public long ChangedTick = NttWorld.Tick;
        public NTT NTT = ntt;
        private int _health = health;
        private int _maxHealth = maxHealth;

        public int Health 
        { 
            readonly get => _health;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _health, value, MsgUserAttribType.Health, NTT);
        }

        public int MaxHealth 
        { 
            readonly get => _maxHealth;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _maxHealth, value, MsgUserAttribType.MaxHealth, NTT);
        }
        
        public override readonly int GetHashCode() => NTT.Id;
    }
}