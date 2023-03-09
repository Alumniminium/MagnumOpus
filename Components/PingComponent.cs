using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct PingComponent
    {
        public int LastPing;
        public int Ping;

        public PingComponent()
        {
            LastPing = 0;
            Ping = 0;
        }
    }
}