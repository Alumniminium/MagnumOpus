using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public readonly struct CqMonsterComponent
    {
        public readonly int CqMonsterId;

        public CqMonsterComponent(int cq_monsterId) => CqMonsterId = cq_monsterId;
    }
}