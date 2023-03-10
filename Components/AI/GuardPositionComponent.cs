using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components.AI
{
    [Component]
    [Save]
    public readonly struct GuardPositionComponent
    {
        public readonly Vector2 Position;
        public GuardPositionComponent(Vector2 pos) => Position = pos;
    }
}