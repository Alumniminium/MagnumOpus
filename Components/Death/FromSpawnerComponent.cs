using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    public readonly struct FromSpawnerComponent
    {
        public readonly int SpawnerId;

        public FromSpawnerComponent(int spawnerId) => SpawnerId = spawnerId;
    }
}