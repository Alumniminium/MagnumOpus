using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct CqActionComponent(long cqAction)
    {
        public long cq_Action = cqAction;
    }
}