using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character head appearance component managing facial features and hairstyles. Contains face ID
/// and hair style with network synchronization for appearance changes. FaceId changes trigger
/// immediate character packet broadcast, while Hair uses the standard network sync helper. 
/// No active systems currently process this component - primarily used for character customization.
/// </summary>
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
        set => NetworkHelper.UpdateSyncedField(ref this, ref _hair, value, MsgUserAttribType.HairStyle, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}