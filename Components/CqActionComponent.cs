using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct CqActionComponent
    {
        public long cq_Action;

        public CqActionComponent(long cqAction) => cq_Action = cqAction;
    }
}