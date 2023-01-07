namespace MagnumOpus.Squiggly
{
    public class ShopDatEntry
    {
        public int ShopId;
        public string Name;
        public int Type;
        public int MoneyType;
        public List<int> Items;

        public ShopDatEntry(int shopId, string name, int type, int moneyType, List<int> items)
        {
            ShopId = shopId;
            Name = name;
            Type = type;
            MoneyType = moneyType;
            Items = items;
        }
    }
}