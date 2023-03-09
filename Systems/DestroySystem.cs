using MagnumOpus.Components.Death;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy", threads: 2) { }

        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
        {
            if (ntt.Type is EntityType.Player or EntityType.Monster)
            {
                var despawn = MsgAction.RemoveEntity(ntt.Id);
                ntt.NetSync(ref despawn, true);
            }
            else if (ntt.Type == EntityType.Item)
            {
                var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                ntt.NetSync(ref delete, true);
            }
            NttWorld.Destroy(in ntt);
            if (IsLogging)
                Logger.Debug("Destroyed {ntt}", ntt);
        }
    }
}