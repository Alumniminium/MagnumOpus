using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct ExpRewardComponent
    {
        public readonly int EntityId;
        public readonly int Experience;

        public ExpRewardComponent(in PixelEntity ntt, int experience)
        {
            EntityId = ntt.Id;
            Experience = experience;
        }
        
        public override int GetHashCode() => EntityId;
    }
}