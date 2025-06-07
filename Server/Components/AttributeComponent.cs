using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct AttributeComponent(in NTT ntt)
    {
        public NTT NTT = ntt;
        private ushort _strength = 0;
        private ushort _agility = 0;
        private ushort _vitality = 0;
        private ushort _spirit = 0;
        private ushort _statPoints = 0;

        public ushort Strength 
        { 
            readonly get => _strength;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _strength, value, MsgUserAttribType.Strength, NTT);
        }

        public ushort Agility 
        { 
            readonly get => _agility;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _agility, value, MsgUserAttribType.Agility, NTT);
        }

        public ushort Vitality 
        { 
            readonly get => _vitality;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _vitality, value, MsgUserAttribType.Vitality, NTT);
        }

        public ushort Spirit 
        { 
            readonly get => _spirit;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _spirit, value, MsgUserAttribType.Spirit, NTT);
        }

        public ushort StatPoints 
        { 
            readonly get => _statPoints;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _statPoints, value, MsgUserAttribType.StatPoints, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}