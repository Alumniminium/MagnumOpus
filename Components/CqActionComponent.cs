using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct CqActionComponent
    {
        public readonly int EntityId;
        public readonly long cq_Action;

        public CqActionComponent(int nttId, long cqAction)
        {
            EntityId = nttId;
            cq_Action = cqAction;
        }

        public override int GetHashCode() => EntityId;
    }
}