using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct InventoryComponent(in NTT ntt, uint money = 0, uint cps = 0)
    {
        public NTT NTT = ntt;
        public long ChangedTick = NttWorld.Tick;

        public Memory<NTT> Items = new NTT[40];

        private uint _money = money;
        private uint _cps = cps;

        public uint Money 
        { 
            readonly get => _money;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _money, value, MsgUserAttribType.MoneyInventory, NTT);
        }

        public uint CPs 
        { 
            readonly get => _cps;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _cps, value, MsgUserAttribType.CPsInventory, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}