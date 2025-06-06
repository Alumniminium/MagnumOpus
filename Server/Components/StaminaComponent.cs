using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct StaminaComponent
    {
        public NTT NTT;
        public long ChangedTick;

        private byte _stamina;
        public byte Stamina
        {
            get => _stamina;
            set
            {
                _stamina = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, _stamina, MsgUserAttribType.Stamina);
                NTT.NetSync(ref packet);
            }
        }

        public byte MaxStamina { get; internal set; }

        public StaminaComponent(in NTT entityId)
        {
            NTT = entityId;
            ChangedTick = NttWorld.Tick;
            Stamina = 0;
            MaxStamina = 100;
        }

        public override int GetHashCode() => NTT.Id;
    }
}
