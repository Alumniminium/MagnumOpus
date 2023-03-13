using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components.AI
{
    [Component]
    [Save]
    public struct GuardPositionComponent
    {
        public Vector2 Position;
        public GuardPositionComponent(Vector2 pos) => Position = pos;
    }
}