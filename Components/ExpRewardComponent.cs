using System.Linq.Expressions;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct ExpRewardComponent
    {
        public readonly int EntityId;
        public int Experience;

        public ExpRewardComponent(in PixelEntity ntt, int experience)
        {
            EntityId = ntt.Id;
            Experience = experience;
        }

        public override int GetHashCode() => EntityId;
    }
}