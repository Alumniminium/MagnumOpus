namespace MagnumOpus.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[ConquerWorld.MaxEntities];

        public static void AddFor(in PixelEntity owner, ref T component)
        {
            Array[owner.Id] = component;
            ConquerWorld.InformChangesFor(in owner);
        }
        public static bool HasFor(in PixelEntity owner) => Array[owner.Id].GetHashCode() == owner.Id;
        public static ref T Get(PixelEntity owner) => ref Array[owner.Id];
        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner, bool notify)
        {
            Array[owner.Id] = new T();
            if (notify)
                ConquerWorld.InformChangesFor(in owner);
        }
    }
}