using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
public struct PickupRequestComponent(in NTT item)
{
    public NTT Item = item;
}