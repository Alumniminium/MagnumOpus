using System.Numerics;

namespace MagnumOpus.Squiggly
{
    public readonly struct CqNpc
    {
        public readonly int UniqueId;
        public readonly Vector2 Location;
        public readonly ushort MapId;
        public readonly ushort Sort;
        public readonly ushort Base;
        public readonly ushort Type;
        public readonly uint Look;
        public readonly string Name;
        public readonly long Task0;
        public readonly long Task1;
        public readonly long Task2;
        public readonly long Task3;
        public readonly long Task4;
        public readonly long Task5;
        public readonly long Task6;
        public readonly long Task7;

        public CqNpc(int uniqueId, Vector2 location, ushort mapId, ushort sort, ushort @base, ushort type, uint look, string name, long task0, long task1, long task2, long task3, long task4, long task5, long task6, long task7)
        {
            UniqueId = uniqueId;
            Location = location;
            MapId = mapId;
            Sort = sort;
            Base = @base;
            Type = type;
            Look = look;
            Name = name;
            Task0 = task0;
            Task1 = task1;
            Task2 = task2;
            Task3 = task3;
            Task4 = task4;
            Task5 = task5;
            Task6 = task6;
            Task7 = task7;
        }
    }
}