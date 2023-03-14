using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct ExpRewardComponent
    {
        public int Experience;

        public ExpRewardComponent(int experience) => Experience = experience;
    }
}