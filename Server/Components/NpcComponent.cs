using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct NpcComponent(ushort baseId, ushort typeId, ushort sort)
{
    public ushort Base = baseId;
    public ushort Type = typeId;
    public ushort Sort = sort;
}