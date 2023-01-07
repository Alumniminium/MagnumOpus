namespace MagnumOpus.Squiggly
{
    public readonly struct CqMonster
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int Look;
        public readonly ushort MaxHealth;
        public readonly ushort Health;
        public readonly int MaximumPhsyicalAttack;
        public readonly int MinimumPhsyicalAttack;
        public readonly int Defense;
        public readonly int Dexterity;
        public readonly int Dodge;
        public readonly Drops Drops;
        public readonly int AttackRange;
        public readonly int ViewRange;
        public readonly int EscapeLife;
        public readonly int AttackSpeed;
        public readonly int WalkSpeed;
        public readonly int RunSpeed;
        public readonly int Level;
        public readonly int AttackUser;
        public readonly int CQAction;
        public readonly int MagicType;
        public readonly int MagicDefense;
        public readonly int MagicHitRate;
        public readonly int AIType;

        public CqMonster(int id, string name, long look, int maximumHp, int currentHp, int maximumPhsyicalAttack, int minimumPhsyicalAttack, int defense, int dexterity, int dodge, Drops drops, int attackRange, int viewRange, int escapeLife, int attackSpeed, int walkSpeed, int runSpeed, int level, int attackUser, int cqAction, int magicType, int magicDefense, int magicHitRate, int aiType)
        {
            Id = id;
            Name = name;
            Look = (int)look;
            MaxHealth = (ushort)maximumHp;
            Health = (ushort)currentHp;
            MaximumPhsyicalAttack = maximumPhsyicalAttack;
            MinimumPhsyicalAttack = minimumPhsyicalAttack;
            Defense = defense;
            Dexterity = dexterity;
            Dodge = dodge;
            Drops = drops;
            AttackRange = attackRange;
            ViewRange = viewRange;
            EscapeLife = escapeLife;
            AttackSpeed = attackSpeed;
            WalkSpeed = walkSpeed;
            RunSpeed = runSpeed;
            Level = level;
            AttackUser = attackUser;
            CQAction = cqAction;
            MagicType = magicType;
            MagicDefense = magicDefense;
            MagicHitRate = magicHitRate;
            AIType = aiType;
        }
    }
}