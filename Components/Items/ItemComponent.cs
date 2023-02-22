using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct ItemComponent
    {
        public readonly int EntityId;
        public int Id;
        public ushort CurrentDurability;
        public ushort MaximumDurability;
        public byte Plus;
        public byte Bless;
        public byte Enchant;
        public byte Gem1;
        public byte Gem2;
        public RebornItemEffect RebornEffect;
        public int CustomTextId;
        public byte StackAmount
        {
            get => (byte)(CustomTextId % 10000000);
            set
            {
                var amount = CustomTextId % 10000000;
                CustomTextId -= amount;
                amount = value;
                CustomTextId += amount;
            }
        }

        public ItemComponent(int id, int itemId, ushort currentDurability, ushort maximumDurability, byte stackAmount, byte plus, byte bless, byte enchant, byte gem1, byte gem2, RebornItemEffect rebornEffect, int customTextId)
        {
            EntityId = id;
            Id = itemId;
            CurrentDurability = currentDurability;
            MaximumDurability = maximumDurability;
            Plus = plus;
            Bless = bless;
            Enchant = enchant;
            Gem1 = gem1;
            Gem2 = gem2;
            RebornEffect = rebornEffect;
            CustomTextId = customTextId;
            StackAmount = stackAmount;
        }
        

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}