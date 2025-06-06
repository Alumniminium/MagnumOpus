using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct CqActionComponent(long cqAction)
    {
        public long cq_Action = cqAction;
    }
}