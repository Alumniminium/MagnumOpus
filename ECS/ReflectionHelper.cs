using System.Reflection;

namespace MagnumOpus.ECS
{
    public static class ReflectionHelper
    {
        private static readonly List<Action<NTT, bool>> RemoveMethodCache;
        private static readonly Dictionary<Type, Action<NTT, bool>> Cache = new();
        static ReflectionHelper()
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Select(t => new { t, aList = t.GetCustomAttributes(typeof(ComponentAttribute), true) })
                .Where(t1 => t1.aList.Length > 0)
                .Select(t1 => t1.t);

            var enumerable = types as Type[] ?? types.ToArray();
            var methods = enumerable.Select(ct => (Action<NTT, bool>)typeof(ComponentList<>).MakeGenericType(ct).GetMethod("Remove")!.CreateDelegate(typeof(Action<NTT, bool>)));

            RemoveMethodCache = new List<Action<NTT, bool>>(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = RemoveMethodCache[i];
                Cache.Add(type, method);
            }
        }
        public static void Remove<T>(in NTT ntt)
        {
            if (!Cache.TryGetValue(typeof(T), out var method))
                return;
            method.Invoke(ntt, true);
        }
        public static void RecycleComponents(in NTT ntt)
        {
            for (var i = 0; i < RemoveMethodCache.Count; i++)
                RemoveMethodCache[i].Invoke(ntt, false);
        }
    }
}