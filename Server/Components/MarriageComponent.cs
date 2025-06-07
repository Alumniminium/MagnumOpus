using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct MarriageComponent(int spouseId, int weddingTick, int divorceTick)
{
    public int SpouseId = spouseId;
    public int WeddingTick = weddingTick;
    public int DivorceTick = divorceTick;
}