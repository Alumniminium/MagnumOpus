using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ExpRewardComponent
    {
        public int Experience;

        public ExpRewardComponent(int experience) => Experience = experience;
    }
}