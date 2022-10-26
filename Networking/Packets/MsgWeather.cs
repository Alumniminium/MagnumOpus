using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MsgWeather
    {
        public ushort Size;
        public ushort Id;
        public WeatherType Type;
        public int Intensity;
        public int Direction;
        public int Color;

        public static unsafe Memory<byte> Create(WeatherType weather, int intensity, int direction, int color)
        {
            var packet = new MsgWeather
            {
                Size = (ushort)sizeof(MsgWeather),
                Id = 1016,
                Type = weather,
                Intensity = intensity,
                Color = color,
                Direction = direction
            };
            return packet;
        }

        public static unsafe implicit operator Memory<byte>(MsgWeather msg)
        {
            var buffer = new byte[sizeof(MsgWeather)];
            fixed (byte* p = buffer)
                *(MsgWeather*)p = *&msg;
            return buffer;
        }
    }
}