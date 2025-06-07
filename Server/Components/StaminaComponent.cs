using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct StaminaComponent(in NTT entityId, byte stamina = 100, byte maxStamina = 100)
    {
        public NTT NTT = entityId;
        public long ChangedTick = NttWorld.Tick;
        private byte _stamina = stamina;

        public byte MaxStamina = maxStamina;

        public byte Stamina
        {
            readonly get => _stamina;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _stamina, value, MsgUserAttribType.Stamina, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}
