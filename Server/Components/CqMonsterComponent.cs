using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct CqMonsterComponent(int cq_monsterId)
    {
        public int CqMonsterId = cq_monsterId;
    }
}