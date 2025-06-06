namespace MagnumOpus.Squiggly
{
    public readonly struct CqItemBonus(int id, int typeId, byte level, short life, short attackMax, short attackMin, short defense, short magicAtk, short magicDef, short dexterity, short dodge)
    {
        public readonly int Id = id;
        public readonly int TypeId = typeId;
        public readonly byte Level = level;
        public readonly short Life = life;
        public readonly short AttackMax = attackMax;
        public readonly short AttackMin = attackMin;
        public readonly short Defense = defense;
        public readonly short MagicAtk = magicAtk;
        public readonly short MagicDef = magicDef;
        public readonly short Dexterity = dexterity;
        public readonly short Dodge = dodge;
    }
}