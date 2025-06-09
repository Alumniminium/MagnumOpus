using System.Runtime.InteropServices;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Networking.Packets;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct MsgAllot
{
    [FieldOffset(0)]
    public ushort Size;
    [FieldOffset(2)]
    public ushort Id;
    [FieldOffset(4)]
    public uint PlayerId;
    [FieldOffset(8)]
    public ushort Points;
    [FieldOffset(10)]
    public ushort AttributeType;

    public static MsgAllot Create(uint playerId, ushort points, ushort attributeType)
    {
        var msg = new MsgAllot
        {
            Size = (ushort)sizeof(MsgAllot),
            Id = 1024,
            PlayerId = playerId,
            Points = points,
            AttributeType = attributeType
        };
        return msg;
    }

    [PacketHandler(PacketId.MsgAllot)]
    public static void Process(NTT ntt, Memory<byte> memory)
    {
        var msg = Co2Packet.Deserialize<MsgAllot>(memory.Span);
        
        if (!ntt.Has<PlayerComponent>())
            return;

        ref var attributes = ref ntt.Get<AttributeComponent>();
        
        // Validate we have enough stat points
        if (attributes.StatPoints < msg.Points)
            return;

        // Apply attribute points based on type
        switch (msg.AttributeType)
        {
            case 0: // Strength
                attributes.Strength += msg.Points;
                break;
            case 1: // Agility  
                attributes.Agility += msg.Points;
                break;
            case 2: // Vitality
                attributes.Vitality += msg.Points;
                break;
            case 3: // Spirit
                attributes.Spirit += msg.Points;
                break;
            default:
                return; // Invalid attribute type
        }

        // Deduct the stat points
        attributes.StatPoints -= msg.Points;
        attributes.ChangedTick = NttWorld.Tick;

        // Echo packet back to confirm allocation
        ntt.NetSync(ref msg);
    }
}