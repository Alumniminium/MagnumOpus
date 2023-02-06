using MagnumOpus.ECS;

namespace MagnumOpus.Components.NetSyncComponents
{
    [Component]
    public readonly struct DirtySyncComponent
    {
        public readonly int EntityId;
        public DirtySyncComponent(int nttId) => EntityId = nttId;
        public override int GetHashCode() => EntityId;
    }
}