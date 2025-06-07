using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct HeadComponent(in NTT ntt, ushort face = 6, ushort hair = 310)
    {
        public NTT NTT = ntt;
        public long ChangedTick = NttWorld.Tick;
        private ushort _hair = hair;
        private ushort _face = face;

        public ushort FaceId
        {
            readonly get => _face;
            set
            {
                _face = value;
                var packet = MsgCharacter.Create(NTT);
                NTT.NetSync(ref packet);
            }
        }

        public ushort Hair
        {
            readonly get => _hair;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _hair, value, MsgUserAttribType.HairStyle, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}