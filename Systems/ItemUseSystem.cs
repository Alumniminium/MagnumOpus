using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using Microsoft.Extensions.Logging;

namespace MagnumOpus.Systems
{
    public sealed class ItemUseSystem : NttSystem<InventoryComponent, RequestItemUseComponent>
    {
        public ItemUseSystem() : base("Item Use", threads: 2) { }

        public override void Update(in NTT ntt, ref InventoryComponent inv, ref RequestItemUseComponent use)
        {
            var destroy = false;
            ref var item = ref NttWorld.GetEntity(use.ItemNetId);
            ref var itemComp = ref item.Get<ItemComponent>();

            if (!Collections.ItemType.TryGetValue(itemComp.Id, out var entry))
            {
                if (IsLogging)
                    FConsole.WriteLine("Item {item} not found in ItemType", item);
                ntt.Remove<RequestItemUseComponent>();
                return;
            }

            if (entry.Action > 0)
            {
                long next = entry.Action;
                while ((next = CqActionProcessor.Process(in ntt, in item, CqProcessor.GetAction(next))) != 0) ;
            }
            else if (entry.Life > 0)
            {
                ref var hlt = ref ntt.Get<HealthComponent>();
                hlt.Health = Math.Clamp(hlt.Health + entry.Life, 0, hlt.MaxHealth);
                destroy = true;
            }
            else if (entry.Mana > 0)
            {
                ref var mna = ref ntt.Get<ManaComponent>();
                mna.Mana = (ushort)Math.Clamp(mna.Mana + entry.Mana, 0, mna.MaxMana);
                destroy = true;
            }

            if (destroy)
                InventoryHelper.RemoveNttFromInventory(ntt, ref inv, item, destroy: true, netSync: true);

            if (IsLogging)
                FConsole.WriteLine("{0} used {1} ({2})", ntt, item, itemComp.Id);
            ntt.Remove<RequestItemUseComponent>();
        }
    }
}