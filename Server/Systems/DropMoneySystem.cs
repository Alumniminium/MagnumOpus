using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles money dropping mechanics including inventory validation, ground placement, and economy tracking.
    /// Manages the creation of money entities on the ground with metrics for server economy monitoring.
    /// </summary>
    public sealed class DropMoneySystem : NttSystem<PositionComponent, InventoryComponent, RequestDropMoneyComponent>
    {
        /// <summary>
        /// Initializes the DropMoneySystem with limited threading and debug logging enabled.
        /// </summary>
        public DropMoneySystem() : base("DropMoney", threads: 2, log: true) { }

        /// <summary>
        /// Processes money drop requests, removing currency from inventory and creating ground money entities.
        /// </summary>
        /// <param name="ntt">The entity dropping the money</param>
        /// <param name="pos">Position component for determining drop location</param>
        /// <param name="inv">Inventory component containing player's money</param>
        /// <param name="drc">Request drop money component specifying amount to drop</param>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref RequestDropMoneyComponent drc)
        {
            if (inv.Money < drc.Amount)
            {
                ntt.Remove<RequestDropMoneyComponent>();
                if (IsLogging)
                    FConsole.WriteLine($"{ntt} tried to drop {drc.Amount} money, but only has {inv.Money}");
                return;
            }

            inv.Money -= (uint)drc.Amount;

            PrometheusPush.MoneyDropCount.Inc();
            PrometheusPush.MoneyDropTotal.Inc(drc.Amount);
            PrometheusPush.ServerExpenses.Inc(drc.Amount);

            var moneyEntity = EntityFactory.MakeMoneyDrop(drc.Amount, ref pos);
            if (moneyEntity != default)
            {
                var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
                moneyEntity.Set(ref spatialUpdate);

                var dropPacket = MsgFloorItem.Create(moneyEntity, Enums.MsgFloorItemType.Create);
                ntt.NetSync(ref dropPacket, true);
                if (IsLogging)
                    FConsole.WriteLine($"{ntt} dropped {drc.Amount} money at {pos.Position}");
            }
            else if (IsLogging)
                FConsole.WriteLine($"Failed to create money drop for {ntt}. Amount: {drc.Amount}, Position: {pos.Position}, Map: {pos.Map}");

            ntt.Remove<RequestDropMoneyComponent>();
        }
    }
}