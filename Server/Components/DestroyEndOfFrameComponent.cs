using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Marker component that schedules an entity for destruction at the end of the current frame.
/// Used to safely clean up entities without causing issues during system processing. Processed
/// by DestroySystem to remove entities from the world. Common usage includes item pickup cleanup,
/// expired entities, and objects that need deferred destruction to maintain system stability.
/// </summary>
public struct DestroyEndOfFrameComponent { }