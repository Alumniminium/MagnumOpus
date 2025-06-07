using MagnumOpus.ECS;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct LifeGiverComponent(in NTT spawnerId)
    {
        public NTT NTT = spawnerId;
    }
}