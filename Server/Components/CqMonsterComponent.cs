using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Monster type identifier component that links entities to their monster definition in the 
/// cq_monstertype database. This component marks an entity as a monster and provides access 
/// to monster-specific data like stats, AI behavior, and loot tables. Used by extension 
/// methods for entity type checking (IsMonster()) and monster spawning systems.
/// </summary>
public struct CqMonsterComponent(int cq_monsterId)
{
    public int CqMonsterId = cq_monsterId;
}