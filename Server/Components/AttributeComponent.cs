using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Core player character attributes component that manages primary stats (Strength, Agility, 
/// Vitality, Spirit) and available stat points. Each attribute change is automatically 
/// network-synchronized to clients. StatPoints are consumed when players allocate them 
/// to increase their attributes through the character development interface.
/// </summary>
public struct AttributeComponent(in NTT ntt)
{
    public NTT NTT = ntt;
    private ushort _strength = 0;
    private ushort _agility = 0;
    private ushort _vitality = 0;
    private ushort _spirit = 0;
    private ushort _statPoints = 0;

    public ushort Strength
    {
        readonly get => _strength;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _strength, value, MsgUserAttribType.Strength, NTT);
    }

    public ushort Agility
    {
        readonly get => _agility;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _agility, value, MsgUserAttribType.Agility, NTT);
    }

    public ushort Vitality
    {
        readonly get => _vitality;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _vitality, value, MsgUserAttribType.Vitality, NTT);
    }

    public ushort Spirit
    {
        readonly get => _spirit;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _spirit, value, MsgUserAttribType.Spirit, NTT);
    }

    public ushort StatPoints
    {
        readonly get => _statPoints;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _statPoints, value, MsgUserAttribType.StatPoints, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}