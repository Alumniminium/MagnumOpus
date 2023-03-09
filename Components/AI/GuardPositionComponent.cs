using System.Numerics;
using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.AI
{
    [Component]
    [Save]
    public readonly struct GuardPositionComponent
    {
        public readonly Vector2 Position;
        [JsonConstructor]
        public GuardPositionComponent(Vector2 pos) => Position = pos;
    }
}