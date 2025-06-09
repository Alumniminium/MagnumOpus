using System.Runtime.InteropServices;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Networking.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MsgAllot
{
    public MsgHeader Header;
    public byte Strength;
    public byte Agility;
    public byte Vitality;
    public byte Spirit;

    [PacketHandler(PacketId.MsgAllot)]
    public static void Process(NTT ntt, Memory<byte> memory)
    {
        var msg = Co2Packet.Deserialize<MsgAllot>(memory.Span);

        var totalPoints = msg.Strength + msg.Agility + msg.Vitality + msg.Spirit;
        if (totalPoints != 1)
            return;

        ref var attributes = ref ntt.Get<AttributeComponent>();
        if (attributes.StatPoints == 0)
            return;

        attributes.Strength += msg.Strength;
        attributes.Agility += msg.Agility;
        attributes.Vitality += msg.Vitality;
        attributes.Spirit += msg.Spirit;
        attributes.StatPoints -= 1;
        attributes.ChangedTick = NttWorld.Tick;
    }
}