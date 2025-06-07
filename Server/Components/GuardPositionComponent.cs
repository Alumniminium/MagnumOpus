using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Guard position component that designates an entity as a guard with a specific patrol/defense
/// location. Guards protect areas within 18 units of their position, attacking non-guard entities
/// that enter their territory before returning to post. Used by GuardAISystem for territorial
/// behavior and by BasicAISystem/AttackSystem for guard identification and filtering.
/// </summary>
public struct GuardPositionComponent(Vector2 pos)
{
    public Vector2 Position = pos;
}