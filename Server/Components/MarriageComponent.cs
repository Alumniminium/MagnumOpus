using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Marriage relationship component storing player matrimonial status and history. Contains
/// spouse entity ID, wedding timestamp, and divorce timestamp for relationship tracking.
/// Currently defined but not actively processed by any systems - represents planned marriage
/// system functionality for social features, relationship benefits, and couple interactions.
/// </summary>
public struct MarriageComponent(int spouseId, int weddingTick, int divorceTick)
{
    public int SpouseId = spouseId;
    public int WeddingTick = weddingTick;
    public int DivorceTick = divorceTick;
}