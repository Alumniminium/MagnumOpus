using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ItemUseSystem : PixelSystem<InventoryComponent, UseItemRequestComponent>
    {
        public ItemUseSystem() : base("ItemUse System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref InventoryComponent inv, ref UseItemRequestComponent use)
        {
            ref var item = ref PixelWorld.GetEntityByNetId(use.ItemNetId);
            ref var itemComp = ref item.Get<ItemComponent>();

            if (Collections.ItemType.TryGetValue(itemComp.Id, out var entry))
            {
                if (entry.Action != 0)
                {
                    long next = entry.Action;
                    while((next = CqActionProcessor.Process(in ntt, CqProcessor.GetAction(next))) != 0);
                }
                else if(entry.Life > 0)
                {
                    ref var hlt = ref ntt.Get<HealthComponent>();
                    hlt.Health = Math.Clamp(hlt.Health + entry.Life, 0, hlt.MaxHealth);
                    hlt.ChangedTick = PixelWorld.Tick;
                }
                else if (entry.Mana > 0)
                {
                    ref var mna = ref ntt.Get<ManaComponent>();
                    mna.Mana = (ushort)Math.Clamp(mna.Mana + entry.Mana, 0, mna.MaxMana);
                    mna.ChangedTick = PixelWorld.Tick;
                }
            }

            ntt.Remove<UseItemRequestComponent>();
        }
    }
}