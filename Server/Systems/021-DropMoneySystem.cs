using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Systems;

public sealed class DropMoneySystem : NttSystem<PositionComponent, InventoryComponent, RequestDropMoneyComponent>
{
    public DropMoneySystem() : base("DropMoney", threads: 1, log: false) { }

    // Handles money dropping from player inventories to the ground with economy tracking. When a
    // player requests to drop money, the system validates they have sufficient funds, deducts the
    // amount from their inventory, and creates a ground money entity. The system also tracks
    // economy metrics via Prometheus for server monitoring and includes spatial hash updates.
    public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref RequestDropMoneyComponent drc)
    {
        // === VALIDATE SUFFICIENT FUNDS ===
        // Ensure player has enough money before processing drop request
        if (inv.Money < drc.Amount)
        {
            if (IsLogging)
                FConsole.WriteLine("{ntt} tried to drop {amount} money, but only has {current}", ntt, drc.Amount, inv.Money);

            ntt.Remove<RequestDropMoneyComponent>();
            return;
        }

        // === DEDUCT MONEY FROM INVENTORY ===
        inv.Money -= (uint)drc.Amount;

        // === CREATE GROUND MONEY ENTITY ===
        var moneyEntity = EntityFactory.MakeMoneyDrop(drc.Amount, ref pos);
        if (moneyEntity == default)
        {
            if (IsLogging)
                FConsole.WriteLine("Failed to create money drop for {ntt}. Amount: {amount}, Position: {pos}, Map: {map}", ntt, drc.Amount, pos.Position, pos.Map);
        }
        else
        {
            // Add to spatial hash for efficient queries
            var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
            moneyEntity.Set(ref spatialUpdate);

            // Broadcast money drop to nearby players
            var dropPacket = MsgFloorItem.Create(moneyEntity, Enums.MsgFloorItemType.Create);
            ntt.NetSync(ref dropPacket, true);

            if (IsLogging)
                FConsole.WriteLine("{ntt} dropped {amount} money at {pos}", ntt, drc.Amount, pos.Position);
        }

        // Clean up the drop request component
        ntt.Remove<RequestDropMoneyComponent>();
    }
}