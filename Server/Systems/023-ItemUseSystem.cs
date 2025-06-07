using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class ItemUseSystem : NttSystem<InventoryComponent, RequestItemUseComponent>
    {
        public ItemUseSystem() : base("Item Use", threads: 1, log: false) { }

        // Handles item usage including consumables for health/mana restoration and items with custom
        // action scripts. Processes item effects by checking item type data, executes CQ action scripts
        // for complex behaviors, restores health/mana for consumables, and removes consumed items
        // from inventory while maintaining proper network synchronization.
        public override void Update(in NTT ntt, ref InventoryComponent inventory, ref RequestItemUseComponent itemUseRequest)
        {
            var shouldDestroy = false;
            ref var itemEntity = ref NttWorld.GetEntity(itemUseRequest.ItemNetId);
            ref var itemComponent = ref itemEntity.Get<ItemComponent>();

            // === VALIDATE ITEM EXISTS ===
            if (!Collections.ItemType.TryGetValue(itemComponent.Id, out var itemTypeEntry))
            {
                if (IsLogging)
                    FConsole.WriteLine("Item {item} not found in ItemType database", itemEntity);
                ntt.Remove<RequestItemUseComponent>();
                return;
            }

            // === PROCESS ITEM EFFECTS ===
            if (itemTypeEntry.Action > 0)
            {
                // Execute custom action script chain
                long nextAction = itemTypeEntry.Action;
                while ((nextAction = CqActionProcessor.Process(in ntt, in itemEntity, CqProcessor.GetAction(nextAction))) != 0) ;
            }
            else if (itemTypeEntry.Life > 0)
            {
                // Health restoration consumable
                ref var healthComponent = ref ntt.Get<HealthComponent>();
                healthComponent.Health = Math.Clamp(healthComponent.Health + itemTypeEntry.Life, 0, healthComponent.MaxHealth);
                shouldDestroy = true;
            }
            else if (itemTypeEntry.Mana > 0)
            {
                // Mana restoration consumable
                ref var manaComponent = ref ntt.Get<ManaComponent>();
                manaComponent.Mana = (ushort)Math.Clamp(manaComponent.Mana + itemTypeEntry.Mana, 0, manaComponent.MaxMana);
                shouldDestroy = true;
            }

            // === CLEANUP CONSUMED ITEMS ===
            if (shouldDestroy)
                InventoryHelper.RemoveNttFromInventory(ntt, ref inventory, itemEntity, destroy: true, netSync: true);

            if (IsLogging)
                FConsole.WriteLine("{player} used {item} (ID: {id})", ntt, itemEntity, itemComponent.Id);

            ntt.Remove<RequestItemUseComponent>();
        }
    }
}