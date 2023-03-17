using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct CombatComponent
    {
        public int MinAttack;
        public int MaxAttack;
        public int Defense;

        public int MagicAttack;
        public int MagicResist;

        public int Dodge;
    }
}
