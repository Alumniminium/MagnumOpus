using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles pickup requests for items and money from the ground, managing inventory space and cleanup.
    /// Processes both money rewards and item transfers with appropriate client notifications.
    /// </summary>
    public sealed class PickupSystem : NttSystem<PositionComponent, InventoryComponent, PickupRequestComponent>
    {
        /// <summary>
        /// Initializes the PickupSystem with limited threading for pickup processing.
        /// </summary>
        public PickupSystem() : base("Pickup", threads: 1, log: false) { }

        /// <summary>
        /// Processes pickup requests, transferring money or items to inventory and cleaning up ground entities.
        /// </summary>
        /// <param name="ntt">The entity picking up the item</param>
        /// <param name="position">Position component for location validation</param>
        /// <param name="inventory">Inventory component to receive picked up items</param>
        /// <param name="pickupRequest">Pickup request component specifying the item to pick up</param>
        public override void Update(in NTT ntt, ref PositionComponent position, ref InventoryComponent inventory, ref PickupRequestComponent pickupRequest)
        {
            if (pickupRequest.Item.Has<MoneyRewardComponent>())
            {
                ref readonly var moneyReward = ref pickupRequest.Item.Get<MoneyRewardComponent>();
                inventory.Money += (uint)moneyReward.Amount;

                var moneyTextMessage = MsgText.Create(in ntt, $"You picked up {moneyReward.Amount} gold", Enums.MsgTextType.TopLeft);
                ntt.NetSync(ref moneyTextMessage);

                if (moneyReward.Amount > 1000)
                {
                    var moneyActionMessage = MsgAction.Create(ntt.Id, moneyReward.Amount, 0, 0, 0, Enums.MsgActionType.GetMoney);
                    ntt.NetSync(ref moneyActionMessage, true);
                }

                pickupRequest.Item.Set<DestroyEndOfFrameComponent>();
            }
            else
            {
                if (!InventoryHelper.HasFreeSpace(ref inventory))
                {
                    ntt.Remove<PickupRequestComponent>();
                    return;
                }

                pickupRequest.Item.Remove<PositionComponent>();
                pickupRequest.Item.Remove<LifeTimeComponent>();
                pickupRequest.Item.Remove<DestroyEndOfFrameComponent>();

                InventoryHelper.AddItem(ntt, ref inventory, in pickupRequest.Item);
                InventoryHelper.SortById(ntt, ref inventory, netSync: true);
            }

            var deleteFloorMessage = MsgFloorItem.Create(in pickupRequest.Item, Enums.MsgFloorItemType.Delete);
            ntt.NetSync(ref deleteFloorMessage, true);

            if (IsLogging)
                FConsole.WriteLine("{0} picked up {1}", ntt, pickupRequest.Item);
            ntt.Remove<PickupRequestComponent>();
        }
    }
}