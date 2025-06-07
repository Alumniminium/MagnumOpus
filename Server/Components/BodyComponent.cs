using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Character appearance component that stores the visual body model ID. The Look field 
    /// corresponds to specific character models (1003 is default), and includes transformed 
    /// states like ghost form (death) or reborn transformations. Changes are automatically 
    /// network-synchronized for real-time appearance updates.
    /// </summary>
    public struct BodyComponent(in NTT ntt, uint look = 1003)
    {
        public NTT NTT = ntt;
        private uint _look = look;

        public uint Look
        {
            readonly get => _look;
            set => NetworkHelper.UpdateSyncedField(ref this, ref _look, value, MsgUserAttribType.Look, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}
