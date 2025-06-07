using System.Drawing;
using NttECS.ECS;
using NttECS.Helpers;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Entity viewport component managing visible entity tracking and spatial awareness. Contains
/// current and previous visible entity sets, viewport rectangle, and change tracking for
/// efficient visibility updates. Not saved to database (no SaveEnabled). Used extensively
/// by AI systems for target acquisition, BoidSystem for flocking neighbors, ViewportSystem
/// for visibility management, and many others. Critical for spatial queries and entity awareness.
/// </summary>
public struct ViewportComponent(float viewDistance)
{
    public long ChangedTick = NttWorld.Tick;
    public HashSet<NTT> EntitiesVisible = [];
    public HashSet<NTT> EntitiesVisibleLast = [];
    private Rectangle _viewport = new(0, 0, (int)viewDistance, (int)viewDistance);

    public Rectangle Viewport
    {
        readonly get => _viewport;
        set => ComponentChangeTracker.UpdateField(ref this, ref _viewport, value);
    }
}