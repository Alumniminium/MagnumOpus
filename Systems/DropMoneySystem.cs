using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropMoneySystem : PixelSystem<PositionComponent, RequestDropMoneyComponent>
    {
        public DropMoneySystem() : base("DropMoney System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref RequestDropMoneyComponent drc)
        {
            if(ntt.Has<InventoryComponent>())
            {
                ref var inv = ref ntt.Get<InventoryComponent>();
                if(inv.Money < drc.Amount)
                {
                    ntt.Remove<RequestDropMoneyComponent>();
                    return;
                }
            }
            
            var itemNtt = PixelWorld.CreateEntity(EntityType.Item);

            int id = 1090000; //Silver
            if (drc.Amount <= 100 && drc.Amount >= 10)
                id = 1090010; //Sycee
            else if (drc.Amount <= 1000 && drc.Amount >= 100)
                id = 1090020; //Gold
            else if (drc.Amount <= 10000 && drc.Amount >= 1000)
                id = 1091010; //GoldBar
            else if (drc.Amount > 10000)
                id = 1091020; //GoldBarsa

            var dropInfo = new ItemComponent(itemNtt.Id, id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var moneyInfo = new MoneyRewardComponent(itemNtt.Id, drc.Amount);

            itemNtt.Set(ref dropInfo);
            itemNtt.Set(ref moneyInfo);

            var dropPos = new PositionComponent(itemNtt.Id, pos.Position, pos.Map);
            itemNtt.Set(ref dropPos);

            var dropMsg = MsgFloorItem.Create(in itemNtt, Enums.MsgFloorItemType.Create);
            ntt.NetSync(ref dropMsg, true);

            Game.Grids[pos.Map].Add(in itemNtt, ref pos);
            ntt.Remove<RequestDropMoneyComponent>();
        }
    }
}