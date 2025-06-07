using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct CombatComponent
    {
        private int _minAttack;
        private int _maxAttack;
        private int _defense;
        private int _magicAttack;
        private int _magicResist;
        private int _dodge;

        public int MinAttack 
        { 
            readonly get => _minAttack;
            set => NetworkSyncHelper.UpdateField(ref this, ref _minAttack, value);
        }

        public int MaxAttack 
        { 
            readonly get => _maxAttack;
            set => NetworkSyncHelper.UpdateField(ref this, ref _maxAttack, value);
        }

        public int Defense 
        { 
            readonly get => _defense;
            set => NetworkSyncHelper.UpdateField(ref this, ref _defense, value);
        }

        public int MagicAttack 
        { 
            readonly get => _magicAttack;
            set => NetworkSyncHelper.UpdateField(ref this, ref _magicAttack, value);
        }

        public int MagicResist 
        { 
            readonly get => _magicResist;
            set => NetworkSyncHelper.UpdateField(ref this, ref _magicResist, value);
        }

        public int Dodge 
        { 
            readonly get => _dodge;
            set => NetworkSyncHelper.UpdateField(ref this, ref _dodge, value);
        }

        public CombatComponent() { }
    }
}
