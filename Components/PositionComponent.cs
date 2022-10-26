using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct PositionComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public Vector3 Position;
        public Vector3 LastPosition;

        public PositionComponent(int nttId, Vector3 position)
        {
            EntityId = nttId;
            Position = position;
            LastPosition = position;
            ChangedTick = PixelWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}