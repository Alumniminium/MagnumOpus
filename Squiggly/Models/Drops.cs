using Co2Core.IO;

namespace MagnumOpus.Squiggly
{
    public readonly struct Drops
    {
        public readonly int Armet;
        public readonly int Armor;
        public readonly int Weapon;
        public readonly int Hp;
        public readonly int Mp;
        public readonly int ItemType;
        public readonly int Money;
        public readonly int Necklace;
        public readonly int Ring;
        public readonly int Shield;
        public readonly int Shoes;
        public readonly HashSet<ItemType.Entry> Items;

        public Drops(int armet, int armor, int weapon, int hp, int mp, int itemType, int money, int necklace, int ring, int shield, int shoes)
        {
            Armet = armet;
            Armor = armor;
            Weapon = weapon;
            Hp = hp;
            Mp = mp;
            ItemType = itemType;
            Money = money;
            Necklace = necklace;
            Ring = ring;
            Shield = shield;
            Shoes = shoes;
            Items = [];
        }
    }
}