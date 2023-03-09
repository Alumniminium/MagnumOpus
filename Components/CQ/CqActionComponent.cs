using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public readonly struct CqActionComponent
    {
        public readonly long cq_Action;

        [JsonConstructor]
        public CqActionComponent(long cqAction) => cq_Action = cqAction;
    }
}