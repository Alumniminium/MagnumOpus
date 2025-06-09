using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class PickupSystem : NttSystem<PositionComponent, InventoryComponent, PickupRequestComponent>
{
    public PickupSystem() : base("Pickup", threads: 1, log: false) { }

    // Handles pickup requests for items and money from the ground, managing inventory space
    // and cleanup. Processes money rewards by adding to inventory and showing pickup messages,
    // handles items by transferring to inventory after space validation, and cleans up ground
    // entities with appropriate client notifications and floor item deletion packets.
    public override void Update(in NTT ntt, ref PositionComponent position, ref InventoryComponent inventory, ref PickupRequestComponent pickupRequest)
    {
        if (pickupRequest.Item.Has<MoneyRewardComponent>())
        {
            // === HANDLE MONEY PICKUP ===
            ref readonly var moneyReward = ref pickupRequest.Item.Get<MoneyRewardComponent>();
            inventory.Money += (uint)moneyReward.Amount;

            // Notify player of money pickup
            var moneyTextMessage = MsgText.Create(in ntt, $"You picked up {moneyReward.Amount} gold", Enums.MsgTextType.TopLeft);
            ntt.NetSync(ref moneyTextMessage);

            // Show money pickup animation for large amounts
            if (moneyReward.Amount > 1000)
            {
                var moneyActionMessage = MsgAction.Create(ntt.Id, moneyReward.Amount, 0, 0, 0, Enums.MsgActionType.GetMoney);
                ntt.NetSync(ref moneyActionMessage, broadcast: true);
            }

            pickupRequest.Item.Set<DestroyEndOfFrameComponent>();
        }
        else
        {
            // === HANDLE ITEM PICKUP ===
            // Validate inventory space before pickup
            if (!InventoryHelper.HasFreeSpace(ref inventory))
            {
                ntt.Remove<PickupRequestComponent>();
                return;
            }

            // Clean up ground-specific components
            pickupRequest.Item.Remove<PositionComponent>();
            pickupRequest.Item.Remove<LifeTimeComponent>();
            pickupRequest.Item.Remove<DestroyEndOfFrameComponent>();

            // Transfer item to inventory and sort
            InventoryHelper.AddItem(ntt, ref inventory, in pickupRequest.Item);
            InventoryHelper.SortById(ntt, ref inventory, netSync: true);
        }

        // === CLEANUP GROUND ENTITY ===
        // Remove floor item visual from clients
        var deleteFloorMessage = MsgFloorItem.Create(in pickupRequest.Item, Enums.MsgFloorItemType.Delete);
        ntt.NetSync(ref deleteFloorMessage, broadcast: true);

        if (IsLogging)
            FConsole.WriteLine("{player} picked up {item}", ntt, pickupRequest.Item);

        ntt.Remove<PickupRequestComponent>();
    }
}