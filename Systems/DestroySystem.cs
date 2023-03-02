using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy", threads: 2) { }

        protected override bool MatchesFilter(in NTT nttId)
        {
            return base.MatchesFilter(nttId);
        }

        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
        {
            if(ntt.Has<ViewportComponent>())
                {
                    if(ntt.Type == EntityType.Player || ntt.Type == EntityType.Monster)
                    {
                        var despawn = MsgAction.RemoveEntity(ntt.NetId);
                        ntt.NetSync(ref despawn, true);
                    }
                    else if (ntt.Type == EntityType.Item)
                    {
                        var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                        ntt.NetSync(ref delete, true);
                    }
                }
            NttWorld.Destroy(in ntt);
            Logger.Debug("Destroyed {ntt}", ntt);
        }
    }
}