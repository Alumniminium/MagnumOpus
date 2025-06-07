using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct EmoteComponent(Emote emote = Emote.Stand)
    {
        public long ChangedTick = NttWorld.Tick;
        private Emote _emote = emote;

        public Emote Emote
        {
            readonly get => _emote;
            set => NetworkSyncHelper.UpdateField(ref this, ref _emote, value);
        }
    }
}