using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct StatusEffectComponent(in NTT entityId, StatusEffect effects = StatusEffect.None)
    {
        public NTT NTT = entityId;
        public long ChangedTick = NttWorld.Tick;
        private StatusEffect _effects = effects;

        public StatusEffect Effects 
        { 
            readonly get => _effects;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _effects, value, MsgUserAttribType.StatusEffect, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}