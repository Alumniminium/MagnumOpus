using Co2Core.IO;
using NttECS.ECS;
using NttECS.Memory;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Magic spell target collection component storing entities affected by area-of-effect spells.
/// Contains list of target entities and magic type data for spell processing. Not saved to
/// database (no SaveEnabled). Used in the magic casting pipeline to collect valid targets
/// before applying spell effects. Part of the magic system's target resolution and spell
/// application workflow for multi-target spells and area effects.
/// </summary>
public struct TargetCollectionComponent(MagicType.Entry magicType)
{
    public SwapList<NTT> Targets = new SwapList<NTT>(4);
    public MagicType.Entry MagicType = magicType;
}