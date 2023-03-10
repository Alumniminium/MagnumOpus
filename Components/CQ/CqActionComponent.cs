using MagnumOpus.ECS;
namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public readonly struct CqActionComponent
    {
        public readonly long cq_Action;

        public CqActionComponent(long cqAction) => cq_Action = cqAction;
    }
}