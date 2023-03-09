using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct MarriageComponent
    {
        public int SpouseId;
        public int WeddingTick;
        public int DivorceTick;

        [JsonConstructor]
        public MarriageComponent(int spouseId, int weddingTick, int divorceTick)
        {
            SpouseId = spouseId;
            WeddingTick = weddingTick;
            DivorceTick = divorceTick;
        }
    }
}