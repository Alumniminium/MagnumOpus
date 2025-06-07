using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Component for tracking the spawner entity that spawned this entity.
/// Used for tracking monster spawner counts and removing spawners when entities are destroyed.
/// </summary>
public struct LifeGiverComponent(in NTT spawnerId)
{
    public NTT NTT = spawnerId;
}