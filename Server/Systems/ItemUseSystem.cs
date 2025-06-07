using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles item usage including consumables for health/mana restoration and items with custom action scripts.
    /// Processes item effects, removes consumed items from inventory, and executes custom item behaviors.
    /// </summary>
    public sealed class ItemUseSystem : NttSystem<InventoryComponent, RequestItemUseComponent>
    {
        /// <summary>
        /// Initializes the ItemUseSystem with limited threading for item usage processing.
        /// </summary>
        public ItemUseSystem() : base("Item Use", threads: 1, log: false) { }

        /// <summary>
        /// Processes item usage requests, applying effects and removing consumable items from inventory.
        /// </summary>
        /// <param name="ntt">The entity using the item</param>
        /// <param name="inventory">Inventory component containing the item</param>
        /// <param name="itemUseRequest">Request item use component specifying which item to use</param>
        public override void Update(in NTT ntt, ref InventoryComponent inventory, ref RequestItemUseComponent itemUseRequest)
        {
            var shouldDestroy = false;
            ref var itemEntity = ref NttWorld.GetEntity(itemUseRequest.ItemNetId);
            ref var itemComponent = ref itemEntity.Get<ItemComponent>();

            if (!Collections.ItemType.TryGetValue(itemComponent.Id, out var itemTypeEntry))
            {
                if (IsLogging)
                    FConsole.WriteLine("Item {item} not found in ItemType", itemEntity);
                ntt.Remove<RequestItemUseComponent>();
                return;
            }

            if (itemTypeEntry.Action > 0)
            {
                long nextAction = itemTypeEntry.Action;
                while ((nextAction = CqActionProcessor.Process(in ntt, in itemEntity, CqProcessor.GetAction(nextAction))) != 0) ;
            }
            else if (itemTypeEntry.Life > 0)
            {
                ref var healthComponent = ref ntt.Get<HealthComponent>();
                healthComponent.Health = Math.Clamp(healthComponent.Health + itemTypeEntry.Life, 0, healthComponent.MaxHealth);
                shouldDestroy = true;
            }
            else if (itemTypeEntry.Mana > 0)
            {
                ref var manaComponent = ref ntt.Get<ManaComponent>();
                manaComponent.Mana = (ushort)Math.Clamp(manaComponent.Mana + itemTypeEntry.Mana, 0, manaComponent.MaxMana);
                shouldDestroy = true;
            }

            if (shouldDestroy)
                InventoryHelper.RemoveNttFromInventory(ntt, ref inventory, itemEntity, destroy: true, netSync: true);

            if (IsLogging)
                FConsole.WriteLine("{0} used {1} ({2})", ntt, itemEntity, itemComponent.Id);
            ntt.Remove<RequestItemUseComponent>();
        }
    }
}