using System.Linq.Expressions;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct ExpRewardComponent
    {
        public readonly int EntityId;
        public int Experience;

        public ExpRewardComponent(in NTT ntt, int experience)
        {
            EntityId = ntt.Id;
            Experience = experience;
        }

        public override int GetHashCode() => EntityId;
    }
}