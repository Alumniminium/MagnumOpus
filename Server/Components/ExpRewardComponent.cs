using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ExpRewardComponent(int experience)
    {
        public int Experience = experience;
    }
}