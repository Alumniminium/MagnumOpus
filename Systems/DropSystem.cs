using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropSystem : PixelSystem<PositionComponent, DropRequestComponent>
    {
        public DropSystem() : base("Drop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref DropRequestComponent drc)
        {
            ref var itemNtt = ref PixelWorld.GetEntity(drc.ItemNetId);
            ref var item = ref itemNtt.Get<ItemComponent>();
            
            var dropPos = new PositionComponent(itemNtt.Id, pos.Position, pos.Map);
            itemNtt.Add(ref dropPos);

            var dropMsg = MsgFloorItem.Create(in itemNtt, Enums.MsgFloorItemType.Create);
            ntt.NetSync(ref dropMsg, true);

            var removeInv = MsgItem.Create(ntt.NetId, itemNtt.NetId, itemNtt.NetId, PixelWorld.Tick, Enums.MsgItemType.RemoveInventory);
            ntt.NetSync(ref removeInv);

            ntt.Remove<DropRequestComponent>();
        }
    }
}