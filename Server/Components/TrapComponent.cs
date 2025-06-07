using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Trap entity marker component that identifies entities as environmental hazards or interactive
/// traps. Empty struct that serves as a tag for entity type classification. Used by extension
/// methods for trap identification (IsTrap()) and potentially by trap-specific systems for
/// damage dealing, activation triggers, and area denial mechanics. Currently defined but
/// not actively processed by any systems - represents planned trap functionality.
/// </summary>
public struct TrapComponent
{
}