using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public  struct EmoteComponent
    {
        public  long ChangedTick;
        public  Emote Emote;

        public EmoteComponent() => ChangedTick = NttWorld.Tick;
        public EmoteComponent(Emote emote = Emote.Stand)
        {
            Emote = emote;
            ChangedTick = NttWorld.Tick;
        }
    }
}