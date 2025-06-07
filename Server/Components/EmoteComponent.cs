using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct EmoteComponent(Emote emote = Emote.Stand)
    {
        public long ChangedTick = NttWorld.Tick;
        private Emote _emote = emote;

        public Emote Emote
        {
            readonly get => _emote;
            set
            {
                if (_emote != value)
                {
                    _emote = value;
                    ChangedTick = NttWorld.Tick;
                }
            }
        }
    }
}