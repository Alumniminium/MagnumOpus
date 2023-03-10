using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    public  struct LifeGiverComponent
    {
        public  NTT NTT;

        public LifeGiverComponent(in NTT spawnerId) => NTT = spawnerId;
    }
}