using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct GuardPositionComponent
    {
        public Vector2 Position;
        public GuardPositionComponent(Vector2 pos) => Position = pos;
    }
}