namespace MagnumOpus.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly T[] Array = new T[PixelWorld.MaxEntities];

        public static void AddFor(in PixelEntity owner, ref T component)
        {
            Array[owner.Id] = component;
            PixelWorld.InformChangesFor(in owner);
        }
        public static bool HasFor(in PixelEntity owner)
        {
            return Array[owner.Id].GetHashCode() == owner.Id;
        }

        public static ref T Get(PixelEntity owner)
        {
            return ref Array[owner.Id];
        }

        // called via reflection @ ReflectionHelper.Remove<T>()
        public static void Remove(PixelEntity owner, bool notify)
        {
            Array[owner.Id] = new T();
            if (notify)
                PixelWorld.InformChangesFor(in owner);
        }
    }
}