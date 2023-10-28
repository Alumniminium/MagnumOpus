using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct EmoteComponent
    {
        public long ChangedTick;
        private Emote _;
        public Emote Emote
        {
            get => _;
            set
            {
                if (value == _)
                    return;

                ChangedTick = NttWorld.Tick;
                _ = value;
            }
        }

        public EmoteComponent() => ChangedTick = NttWorld.Tick;
        public EmoteComponent(Emote emote = Emote.Stand)
        {
            Emote = emote;
            ChangedTick = NttWorld.Tick;
        }
    }
}