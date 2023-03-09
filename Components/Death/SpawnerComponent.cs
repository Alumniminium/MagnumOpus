using System.Drawing;
using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public struct SpawnerComponent
    {
        public readonly int GeneratorId;
        public readonly int MonsterId;
        public readonly Rectangle SpawnArea;
        public readonly int MaxCount;
        public readonly int TimerSeconds;
        public readonly int GenPerTimer;

        public long RunTick;
        public int Count;

        [JsonConstructor]
        public SpawnerComponent(long generatorId, in Rectangle spawnArea, int monsterId, int spawnLimit, int spawnInterval, int spawnCount)
        {
            GeneratorId = (int)generatorId;
            MonsterId = monsterId;
            SpawnArea = spawnArea;
            MaxCount = spawnLimit;
            TimerSeconds = spawnInterval;
            GenPerTimer = spawnCount;
            RunTick = NttWorld.TargetTps * TimerSeconds;
        }
    }
}