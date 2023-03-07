using System.Runtime.CompilerServices;

namespace MagnumOpus.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[NttWorld.MaxEntities];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT owner, ref T component)
        {
            if(Array[owner.Id].GetHashCode() == 0)
                NttWorld.InformChangesFor(in owner);
            Array[owner.Id] = component;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFor(in NTT owner) => Array[owner.Id].GetHashCode() != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(NTT owner) => ref Array[owner.Id];
        // called via reflection @ ReflectionHelper.Remove<T>()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(NTT owner, bool notify)
        {
            if (Array[owner.Id].GetHashCode() == owner.Id && notify)
                NttWorld.InformChangesFor(in owner);

            Array[owner.Id] = default;
        }

        // called via reflection @ ReflectionHelper.Save<T>()
        public static void Save(string path) { }
    }
}