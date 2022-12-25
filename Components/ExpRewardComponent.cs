using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct ExpRewardComponent
    {
        public readonly int EntityId;
        public readonly uint Experience;

        public ExpRewardComponent(in PixelEntity ntt, uint experience)
        {
            EntityId = ntt.Id;
            Experience = experience;
        }
        
        public override int GetHashCode() => EntityId;
    }
}