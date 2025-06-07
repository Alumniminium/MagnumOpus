using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct CqMonsterComponent(int cq_monsterId)
{
    public int CqMonsterId = cq_monsterId;
}