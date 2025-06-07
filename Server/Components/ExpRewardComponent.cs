using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Experience reward component that grants experience points to entities. Contains the amount
/// of experience to be awarded. Processed by LevelingSystem to increase entity experience,
/// handle level progression, and apply automatic attribute allocations. Also used by DamageSystem
/// and TeamSystem to distribute experience rewards from monster kills to players and party members.
/// </summary>
public struct ExpRewardComponent(int experience)
{
    public int Experience = experience;
}