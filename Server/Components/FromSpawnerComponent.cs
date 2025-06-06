using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LifeGiverComponent(in NTT spawnerId)
    {
        public NTT NTT = spawnerId;
    }
}