using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BodyComponent(in NTT ntt, uint look = 1003)
    {
        public NTT NTT = ntt;
        private uint _look = look;

        public uint Look 
        { 
            readonly get => _look;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _look, value, MsgUserAttribType.Look, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}
