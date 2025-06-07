using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character class/profession component managing player class identity and development paths.
/// Contains class type (Trojan, Archer, Warrior, etc.) with automatic network synchronization
/// for class changes. Used by LevelingSystem for profession-specific attribute allocation and
/// TeamSystem for class verification. Central to character build systems, skill access, equipment
/// restrictions, and class-based gameplay mechanics throughout the game.
/// </summary>
public struct ProfessionComponent(in NTT ntt, ClasseName profession = ClasseName.Trojan)
{
    public NTT NTT = ntt;
    public long ChangedTick = NttWorld.Tick;
    private ClasseName _profession = profession;

    public ClasseName Profession
    {
        readonly get => _profession;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _profession, value, MsgUserAttribType.Class, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}