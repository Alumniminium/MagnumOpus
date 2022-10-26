using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct EmoteComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public Emote Emote;

        public EmoteComponent(int entityId, Emote emote = Emote.Stand)
        {
            EntityId = entityId;
            Emote = emote;
            ChangedTick = PixelWorld.Tick;
        }
        
        public override int GetHashCode() => EntityId;
    }
}