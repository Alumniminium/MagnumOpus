using MagnumOpus.ECS;
using MagnumOpus.SourceGeneration;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct ManaComponent
    {
        public long ChangedTick = NttWorld.Tick;
        
        private ushort _mana;
        private ushort _maxMana;

        public ManaComponent() { }
        
        public ManaComponent(ushort mana, ushort maxMana)
        {
            Mana = mana;        // Uses generated property
            MaxMana = maxMana;  // Uses generated property
        }
    }
}