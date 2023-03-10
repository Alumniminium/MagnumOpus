using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct StatusEffectComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        private StatusEffect _effects;
        public StatusEffect Effects
        {
            get => _effects;
            set
            {
                _effects = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, (uint)_effects, MsgUserAttribType.StatusEffect);
                ntt.NetSync(ref packet, true);
            }
        }

        public StatusEffectComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Effects = StatusEffect.None;
        }

        public override int GetHashCode() => EntityId;
    }
}