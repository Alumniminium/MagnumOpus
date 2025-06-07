using System.Drawing;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Monster spawner component that manages automatic entity generation in defined areas.
/// Contains generator ID, monster type, spawn area rectangle, spawn limits, timing intervals,
/// and current spawn counts. Used by MonsterRespawnSystem to continuously populate game areas
/// with monsters, managing spawn rates, population limits, and spawn timing. Also used by
/// DeathSystem to track spawner relationships. Essential for world population and PvE content.
/// </summary>
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