using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.NetSyncComponents
{
    [Component]
    public readonly struct DirSyncComponent
    {
        public readonly int EntityId;
        public readonly Direction Direction;

        public DirSyncComponent(int nttId, Direction direction)
        {
            EntityId = nttId;
            Direction = direction;
        }

        public override int GetHashCode() => EntityId;
    }
}