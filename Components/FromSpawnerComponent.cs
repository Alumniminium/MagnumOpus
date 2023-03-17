using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LifeGiverComponent
    {
        public NTT NTT;

        public LifeGiverComponent(in NTT spawnerId) => NTT = spawnerId;
    }
}