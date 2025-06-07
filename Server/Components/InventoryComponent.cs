using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player inventory component managing item storage, money, and CPs (Conquer Points). Contains
/// a 40-slot item array, currency values with network synchronization, and change tracking.
/// Extensively used by multiple systems: EquipSystem for item management, ShopSystem for
/// transactions, ItemUseSystem for consumption, PickupSystem for item collection, DropSystems
/// for item dropping, and DeathSystem for death penalties. Core component for player economy.
/// </summary>
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
        set => NetworkHelper.UpdateSyncedField(ref this, ref _money, value, MsgUserAttribType.MoneyInventory, NTT);
    }

    public uint CPs
    {
        readonly get => _cps;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _cps, value, MsgUserAttribType.CPsInventory, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}