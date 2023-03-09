using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    public readonly struct FromSpawnerComponent
    {
        public readonly int SpawnerId;

        [JsonConstructor]
        public FromSpawnerComponent(int spawnerId) => SpawnerId = spawnerId;
    }
}