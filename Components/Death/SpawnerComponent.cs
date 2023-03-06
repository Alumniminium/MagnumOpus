using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Components;

[Component]
public struct SpawnerComponent
{
    public readonly int EntityId;

    public readonly int GeneratorId;
    public readonly int MonsterId;
    public readonly Rectangle SpawnArea;
    public readonly int MaxCount;
    public readonly int TimerSeconds;
    public readonly int GenPerTimer;

    public long RunTick;
    public int Count;

    public SpawnerComponent(in NTT ntt, long generatorId, in Rectangle spawnArea, int monsterId, int spawnLimit, int spawnInterval, int spawnCount)
    {
        EntityId = ntt.Id;
        GeneratorId = (int)generatorId;
        MonsterId = monsterId;
        SpawnArea = spawnArea;
        MaxCount = spawnLimit;
        TimerSeconds = spawnInterval;
        GenPerTimer = spawnCount;
        RunTick = NttWorld.TargetTps * TimerSeconds;
    }

    public override int GetHashCode() => EntityId;
}