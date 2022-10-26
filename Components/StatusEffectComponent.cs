using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct StatusEffectComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        
        public Enums.StatusEffect Effects;

        public StatusEffectComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Effects = Enums.StatusEffect.None;
        }

        public override int GetHashCode() => EntityId;
    }
}