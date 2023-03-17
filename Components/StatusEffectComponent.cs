using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components
{



    [Component(saveEnabled: true)]
    public struct StatusEffectComponent
    {
        public NTT NTT;
        public long ChangedTick;

        private StatusEffect _effects;
        public StatusEffect Effects
        {
            get => _effects;
            set
            {
                _effects = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, (uint)_effects, MsgUserAttribType.StatusEffect);
                NTT.NetSync(ref packet, true);
            }
        }

        public StatusEffectComponent(in NTT entityId)
        {
            NTT = entityId;
            ChangedTick = NttWorld.Tick;
            Effects = StatusEffect.None;
        }

        public override int GetHashCode() => NTT.Id;
    }
}