using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct MarriageComponent
    {
        public int SpouseId;
        public int WeddingTick;
        public int DivorceTick;

        public MarriageComponent(int spouseId, int weddingTick, int divorceTick)
        {
            SpouseId = spouseId;
            WeddingTick = weddingTick;
            DivorceTick = divorceTick;
        }
    }
}