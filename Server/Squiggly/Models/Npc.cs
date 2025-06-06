using System.Numerics;

namespace MagnumOpus.Squiggly
{
    public readonly struct CqNpc(int uniqueId, Vector2 location, ushort mapId, ushort sort, ushort @base, ushort type, uint look, string name, long task0, long task1, long task2, long task3, long task4, long task5, long task6, long task7)
    {
        public readonly int UniqueId = uniqueId;
        public readonly Vector2 Location = location;
        public readonly ushort MapId = mapId;
        public readonly ushort Sort = sort;
        public readonly ushort Base = @base;
        public readonly ushort Type = type;
        public readonly uint Look = look;
        public readonly string Name = name;
        public readonly long Task0 = task0;
        public readonly long Task1 = task1;
        public readonly long Task2 = task2;
        public readonly long Task3 = task3;
        public readonly long Task4 = task4;
        public readonly long Task5 = task5;
        public readonly long Task6 = task6;
        public readonly long Task7 = task7;
    }
}