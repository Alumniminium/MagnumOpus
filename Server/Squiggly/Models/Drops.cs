using Co2Core.IO;

namespace MagnumOpus.Squiggly
{
    public readonly struct Drops(int armet, int armor, int weapon, int hp, int mp, int itemType, int money, int necklace, int ring, int shield, int shoes)
    {
        public readonly int Armet = armet;
        public readonly int Armor = armor;
        public readonly int Weapon = weapon;
        public readonly int Hp = hp;
        public readonly int Mp = mp;
        public readonly int ItemType = itemType;
        public readonly int Money = money;
        public readonly int Necklace = necklace;
        public readonly int Ring = ring;
        public readonly int Shield = shield;
        public readonly int Shoes = shoes;
        public readonly HashSet<ItemType.Entry> Items = [];
    }
}