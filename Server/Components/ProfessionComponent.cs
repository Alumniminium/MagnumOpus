using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ProfessionComponent(in NTT ntt, ClasseName profession = ClasseName.Trojan)
    {
        public NTT NTT = ntt;
        public long ChangedTick = NttWorld.Tick;
        private ClasseName _profession = profession;

        public ClasseName Profession 
        { 
            readonly get => _profession;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _profession, value, MsgUserAttribType.Class, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}