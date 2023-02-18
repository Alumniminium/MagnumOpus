namespace MagnumOpus.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[NttWorld.MaxEntities];

        public static void AddFor(in NTT owner, ref T component)
        {
            Array[owner.Id] = component;
            NttWorld.InformChangesFor(in owner);
        }
        public static bool HasFor(in NTT owner) => Array[owner.Id].GetHashCode() != 0;
        public static ref T Get(NTT owner) => ref Array[owner.Id];
        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(NTT owner, bool notify)
        {
            Array[owner.Id] = default;
            if (notify)
                NttWorld.InformChangesFor(in owner);
        }
    }
}