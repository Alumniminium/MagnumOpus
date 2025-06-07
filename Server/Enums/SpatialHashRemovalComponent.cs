using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: false)]
public struct SpatialHashUpdateComponent(Vector2 pos, Vector2 lastPos, int map, int lastMap, SpacialHashUpdatType type)
{
    public Vector2 Position = pos;
    public Vector2 LastPosition = lastPos;
    public int LastMap = lastMap;
    public int Map = map;
    public SpacialHashUpdatType Type = type;
}
