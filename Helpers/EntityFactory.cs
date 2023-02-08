using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Helpers
{
    public static class EntityFactory
    {
        public static ref NTT MakeDefaultItem(int itemId, out bool success, Vector2 pos = default, int map = 0, bool randomDurability = false)
        {
            ref var ntt = ref NttWorld.CreateEntity(EntityType.Item);

            if(Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
            {
                success = false;
                return ref ntt;
            }
            var durability = randomDurability ? (ushort)(1 + Random.Shared.NextSingle() * itemType.AmountLimit) : itemType.AmountLimit;

            var itemInfo = new ItemComponent(ntt.Id, itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
            ntt.Set(ref itemInfo);

            if(pos != Vector2.Zero && map != 0)
            {
                var posInfo = new PositionComponent(ntt.Id, pos, map);
                ntt.Set(ref posInfo);
            }

            success = true;
            return ref ntt;
        }

        public static ref NTT MakeMoneyDrop(int amount, ref PositionComponent pos, out bool success)
        {
            var itemId = ItemHelper.GetItemIdFromMoney(amount);
            ref var ntt = ref MakeDefaultItem(itemId, out success, pos.Position, pos.Map);
            if(success == false)
                return ref ntt;

             var moneyInfo = new MoneyRewardComponent(ntt.Id, amount);
            ntt.Set(ref moneyInfo);

            return ref ntt;
        }      
    }
}