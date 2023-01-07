namespace MagnumOpus.Squiggly
{
    public readonly struct CqItemBonus
    {
        public readonly int Id;
        public readonly int TypeId;
        public readonly byte Level;
        public readonly short Life;
        public readonly short AttackMax;
        public readonly short AttackMin;
        public readonly short Defense;
        public readonly short MagicAtk;
        public readonly short MagicDef;
        public readonly short Dexterity;
        public readonly short Dodge;

        public CqItemBonus(int id, int typeId, byte level, short life, short attackMax, short attackMin, short defense, short magicAtk, short magicDef, short dexterity, short dodge)
        {
            Id = id;
            TypeId = typeId;
            Level = level;
            Life = life;
            AttackMax = attackMax;
            AttackMin = attackMin;
            Defense = defense;
            MagicAtk = magicAtk;
            MagicDef = magicDef;
            Dexterity = dexterity;
            Dodge = dodge;
        }
    }
}