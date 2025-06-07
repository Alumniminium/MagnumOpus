using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Legacy Conquer Online action script reference component that stores a cq_action database 
/// ID for scripted behaviors. Used with CqActionProcessor to execute action chains for NPCs, 
/// items, and interactive objects. Found in DeathSystem where it links entities to their 
/// death script behaviors for rewards, effects, and cleanup logic.
/// </summary>
public struct CqActionComponent(long cqAction)
{
    public long cq_Action = cqAction;
}