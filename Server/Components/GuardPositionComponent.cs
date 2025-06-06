using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct GuardPositionComponent(Vector2 pos)
    {
        public Vector2 Position = pos;
    }
}