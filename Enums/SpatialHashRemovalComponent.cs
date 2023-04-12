using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: false)]
    public struct SpatialHashUpdateComponent
    {
        public Vector2 Position;
        public Vector2 LastPosition;
        public int Map;
        public SpacialHashUpdatType Type;

        public SpatialHashUpdateComponent(Vector2 pos, Vector2 lastPos, int map, SpacialHashUpdatType type)
        {
            Position = pos;
            LastPosition = lastPos;
            Map = map;
            Type = type;
        }
    }
}
