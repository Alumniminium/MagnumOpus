using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct EmoteComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private Emote _emote;

        public EmoteComponent() { }
        public EmoteComponent(Emote emote = Emote.Stand)
        {
            Emote = emote;  // Uses generated property
        }
    }
}