using System.Drawing;
using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public struct SpawnerComponent
    {
        public int GeneratorId;
        public int MonsterId;
        public Rectangle SpawnArea;
        public int MaxCount;
        public int TimerSeconds;
        public int GenPerTimer;

        public long RunTick;
        public int Count;

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