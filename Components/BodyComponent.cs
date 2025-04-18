using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BodyComponent
    {
        public NTT ntt;
        public long ChangedTick;
        private uint look = 1003;
        public uint Look
        {
            get => look;
            set
            {
                look = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(ntt.Id, look, MsgUserAttribType.Look);
                ntt.NetSync(ref packet, true);
            }
        }

        public BodyComponent() => ChangedTick = NttWorld.Tick;

        public BodyComponent(in NTT ntt, uint look = 1003)
        {
            this.ntt = ntt;
            ChangedTick = NttWorld.Tick;
            Look = look;
        }

        public override int GetHashCode() => ntt;
    }
}
