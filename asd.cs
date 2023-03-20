using System;
using System.IO;
using System.Text;


namespace BubbleChat.Packets;

public class MessagePacket
{
    private readonly MemoryStream memoryStream;

    public MessagePacket(byte packetId)
    {
        memoryStream = new MemoryStream();
        memoryStream.Seek(2, SeekOrigin.Begin);
        memoryStream.WriteByte(packetId);
    }

    public MessagePacket WriteAuthorId(byte id)
    {
        memoryStream.Seek(3, SeekOrigin.Begin);
        memoryStream.WriteByte(id);
        return this;
    }

    public MessagePacket WriteMessage(string message)
    {
        memoryStream.Seek(4, SeekOrigin.Begin);
        memoryStream.Write(Encoding.ASCII.GetBytes(message));
        return this;
    }

    public byte[] Finalize()
    {
        var size = (ushort)memoryStream.Position;
        memoryStream.Seek(0, SeekOrigin.Begin);
        memoryStream.Write(BitConverter.GetBytes(size));
        memoryStream.Flush();
        var buffer = memoryStream.ToArray();

        memoryStream.Dispose();
        return buffer;
    }
}