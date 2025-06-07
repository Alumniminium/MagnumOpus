using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character stamina/energy component managing physical endurance for actions like running,
/// combat, and special abilities. Contains current and maximum stamina with automatic network
/// synchronization for real-time stamina updates. Currently defined but not actively processed
/// by any systems - represents planned stamina system for action costs, movement speed
/// restrictions, and physical ability limitations based on endurance management.
/// </summary>
public struct StaminaComponent(in NTT entityId, byte stamina = 100, byte maxStamina = 100)
{
    public NTT NTT = entityId;
    public long ChangedTick = NttWorld.Tick;
    private byte _stamina = stamina;

    public byte MaxStamina = maxStamina;

    public byte Stamina
    {
        readonly get => _stamina;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _stamina, value, MsgUserAttribType.Stamina, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}
