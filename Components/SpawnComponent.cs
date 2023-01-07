namespace MagnumOpus.Components
{
    public readonly struct SpawnComponent
    {
        public readonly int EntityId;
        public readonly int SpawnId;

        public SpawnComponent(int entityId, int spawnId)
        {
            EntityId = entityId;
            SpawnId = spawnId;
        }

        public override int GetHashCode() => EntityId;
    }
}