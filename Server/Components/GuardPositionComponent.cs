using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct GuardPositionComponent(Vector2 pos)
    {
        public Vector2 Position = pos;
    }
}