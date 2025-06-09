using System.Numerics;
using MagnumOpus.AOGP;
using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

/// <summary>
/// Core AI component managing behavioral state and decision-making for intelligent entities.
/// Integrates with Goal-Oriented Action Planning (GOAP) system to create reactive, goal-driven behavior.
/// 
/// Used by BasicAISystem and GuardAISystem to control monster behavior patterns including
/// target acquisition, action planning, state transitions, and sleep cycles for performance optimization.
/// 
/// **Architecture Warning**: Contains reference types (Lists) that violate ECS data-oriented design.
/// This component contributes to garbage collection pressure and should be refactored to use
/// fixed-size arrays or entity-based action storage for better performance.
/// 
/// **State Flow**: Idle → WakingUp → Planning → Executing → Sleeping → repeat
/// </summary>
[Component(SaveEnabled: true)]
public struct BrainComponent(params GOAPAction[] actions)
{
    /// <summary>Current behavioral state controlling AI decision flow</summary>
    public BrainState State = BrainState.Idle;
    
    /// <summary>Entity reference to current target (player, monster, etc.)</summary>
    public NTT Target;
    
    /// <summary>Last known world position of the target for movement planning</summary>
    public Vector2 TargetPosition = Vector2.Zero;
    
    /// <summary>Sleep countdown timer in game ticks before next AI update (performance optimization)</summary>
    public int SleepTicks = 0;
    
    /// <summary>Current GOAP action sequence being executed. WARNING: Causes GC pressure</summary>
    public List<GOAPAction> Plan = [];
    
    /// <summary>Actions this entity can perform for GOAP planning. WARNING: Violates ECS principles</summary>
    public List<GOAPAction> AvailableActions = new List<GOAPAction>(actions);
}