using System.Drawing;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
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