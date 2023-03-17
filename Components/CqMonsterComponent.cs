using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct CqMonsterComponent
    {
        public int CqMonsterId;

        public CqMonsterComponent(int cq_monsterId) => CqMonsterId = cq_monsterId;
    }
}