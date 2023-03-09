using System.Numerics;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Helpers
{
    public static class EntityFactory
    {
        public static NTT? MakeDefaultItem(int itemId, Vector2 pos = default, int map = 0, bool randomDurability = false)
        {
            if (Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
                return null;

            var durability = randomDurability ? (ushort)(1 + (Random.Shared.NextSingle() * itemType.AmountLimit)) : itemType.AmountLimit;

            ref var ntt = ref NttWorld.CreateEntity(EntityType.InvItem);
            var itemInfo = new ItemComponent(itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
            ntt.Set(ref itemInfo);

            if (pos != Vector2.Zero && map != 0)
            {
                var posInfo = new PositionComponent(pos, map);
                ntt.Set(ref posInfo);
            }
            return ntt;
        }

        public static NTT? MakeMoneyDrop(int amount, ref PositionComponent pos)
        {
            var itemId = ItemHelper.GetItemIdFromMoney(amount);
            var ntt = MakeDefaultItem(itemId, pos.Position, pos.Map);
            if (ntt == null)
                return null;

            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(30));
            var vwp = new ViewportComponent(18f);
            ntt.Value.Set(ref vwp);
            ntt.Value.Set(ref ltc);

            var moneyInfo = new MoneyRewardComponent(amount);
            ntt.Value.Set(ref moneyInfo);

            return ntt;
        }
    }
}