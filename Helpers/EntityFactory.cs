using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Helpers
{
    public static class EntityFactory
    {
        public static NTT? MakeDefaultItem(int itemId, Vector2 pos = default, int map = 0, bool randomDurability = false)
        {
            if(Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
                return null;
            
            var durability = randomDurability ? (ushort)(1 + Random.Shared.NextSingle() * itemType.AmountLimit) : itemType.AmountLimit;

            ref var ntt = ref NttWorld.CreateEntity(EntityType.Item);
            var itemInfo = new ItemComponent(ntt.Id, itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
            ntt.Set(ref itemInfo);

            if(pos != Vector2.Zero && map != 0)
            {
                var posInfo = new PositionComponent(ntt.Id, pos, map);
                ntt.Set(ref posInfo);
            }
            return ntt;
        }

        public static NTT? MakeMoneyDrop(int amount, ref PositionComponent pos)
        {
            var itemId = ItemHelper.GetItemIdFromMoney(amount);
            var ntt = MakeDefaultItem(itemId, pos.Position, pos.Map);
            if(ntt == null)
                return null;

            var ltc = new LifeTimeComponent(ntt.Value.Id, TimeSpan.FromSeconds(30));
            var vwp = new ViewportComponent(ntt.Value.Id, 18f);
            ntt.Value.Set(ref vwp);
            ntt.Value.Set(ref ltc);

             var moneyInfo = new MoneyRewardComponent(ntt.Value.Id, amount);
            ntt.Value.Set(ref moneyInfo);

            return ntt;
        }      
    }
}