namespace MagnumOpus.Squiggly
{
    public class ShopDatEntry(int shopId, string name, int type, int moneyType, List<int> items)
    {
        public int ShopId = shopId;
        public string Name = name;
        public int Type = type;
        public int MoneyType = moneyType;
        public List<int> Items = items;
    }
}