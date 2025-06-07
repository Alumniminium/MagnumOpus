using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct ExpRewardComponent(int experience)
{
    public int Experience = experience;
}