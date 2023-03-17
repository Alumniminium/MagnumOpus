using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct CqActionComponent
    {
        public long cq_Action;

        public CqActionComponent(long cqAction) => cq_Action = cqAction;
    }
}