using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct PingComponent
    {
        public int LastPing;
        public int Ping;
    }
}