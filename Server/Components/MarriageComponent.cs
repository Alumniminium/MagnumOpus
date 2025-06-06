using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct MarriageComponent(int spouseId, int weddingTick, int divorceTick)
    {
        public int SpouseId = spouseId;
        public int WeddingTick = weddingTick;
        public int DivorceTick = divorceTick;
    }
}