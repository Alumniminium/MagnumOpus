using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct EmoteComponent
    {
        public readonly int EntityId;
        public readonly long ChangedTick;
        public readonly Emote Emote;

        public EmoteComponent(int entityId, Emote emote = Emote.Stand)
        {
            EntityId = entityId;
            Emote = emote;
            ChangedTick = PixelWorld.Tick;
        }
        
        public override int GetHashCode() => EntityId;
    }
}