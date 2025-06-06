using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct CombatComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private int _minAttack;
        private int _maxAttack;
        private int _defense;
        private int _magicAttack;
        private int _magicResist;
        private int _dodge;

        public CombatComponent() { }
    }
}
