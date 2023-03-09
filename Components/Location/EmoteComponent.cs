using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public readonly struct EmoteComponent
    {
        public readonly long ChangedTick;
        public readonly Emote Emote;

        [JsonConstructor]
        public EmoteComponent(Emote emote = Emote.Stand)
        {
            Emote = emote;
            ChangedTick = NttWorld.Tick;
        }
    }
}