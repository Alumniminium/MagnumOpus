using MagnumOpus.ECS;

namespace MagnumOpus.Components.Death
{
    [Component]
    public readonly struct FromSpawnerComponent
    {
        public readonly int EntityId;
        public readonly int SpawnerId;

        public FromSpawnerComponent(int entityId, int spawnerId)
        {
            EntityId = entityId;
            SpawnerId = spawnerId;
        }

        public override int GetHashCode() => EntityId;
    }
}