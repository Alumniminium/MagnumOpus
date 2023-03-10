using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct HeadComponent
    {
        public  NTT NTT;
        public long ChangedTick;

        private ushort hair;
        public ushort FaceId;

        internal ushort Hair
        {
            get => hair; set
            {
                hair = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT, hair, MsgUserAttribType.HairStyle);
                NTT.NetSync(ref packet, true);
            }
        }

        public HeadComponent() => ChangedTick = NttWorld.Tick;
        public HeadComponent(in NTT ntt, ushort face = 6, ushort hair = 310)
        {
            NTT = ntt;
            ChangedTick = NttWorld.Tick;
            Hair = hair;
            FaceId = face;
        }

        public override int GetHashCode() => NTT.Id;
    }
}