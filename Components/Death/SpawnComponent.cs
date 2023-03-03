using MagnumOpus.ECS;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct SpawnComponent
    {
        public readonly int EntityId;
        public readonly cq_generator Spawn;

        public SpawnComponent(int entityId, cq_generator spawn)
        {
            EntityId = entityId;
            Spawn = spawn;
        }

        public override int GetHashCode() => EntityId;
    }
}