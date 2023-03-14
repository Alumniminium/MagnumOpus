using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct LifeGiverComponent
    {
        public NTT NTT;

        public LifeGiverComponent(in NTT spawnerId) => NTT = spawnerId;
    }
}