using System.Reflection;

namespace MagnumOpus.ECS
{
    public static class ReflectionHelper
    {
        private static readonly List<Action<NTT, bool>> RemoveMethodCache = new();
        private static readonly Dictionary<Type, Action<NTT, bool>> RemoveCache = new();        
        
        private static readonly List<Action<string>> SaveMethodCache = new();
        private static readonly Dictionary<Type, Action<string>> SaveCache = new();
        static ReflectionHelper()
        {
            GetRemoveMethods();
            GetSaveMethods();
        }

        private static void GetRemoveMethods()
        {
            var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Select(t => new { t, aList = t.GetCustomAttributes(typeof(ComponentAttribute), true) })
                            .Where(t1 => t1.aList.Length > 0)
                            .Select(t1 => t1.t);

            var enumerable = types as Type[] ?? types.ToArray();
            var methods = enumerable.Select(ct => (Action<NTT, bool>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Remove")!.CreateDelegate(typeof(Action<NTT, bool>)));

            RemoveMethodCache.AddRange(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = RemoveMethodCache[i];
                RemoveCache.Add(type, method);
            }
        }

        private static void GetSaveMethods()
        {
            var enumerable = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Select(t => new { t, l = t.GetCustomAttributes(typeof(SaveAttribute), true) })
                        .Where(t => t.l.Length > 0)
                        .Select(t => t.t)
                        .ToList();

            var methods = enumerable.Select(ct => (Action<string>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Save")!.CreateDelegate(typeof(Action<string>)));

            SaveMethodCache.AddRange(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = SaveMethodCache[i];
                SaveCache.Add(type, method);
            }
        }   

        public static void Remove<T>(in NTT ntt)
        {
            if (!RemoveCache.TryGetValue(typeof(T), out var method))
                return;
            method.Invoke(ntt, true);
        }
        public static void RecycleComponents(in NTT ntt)
        {
            for (var i = 0; i < RemoveMethodCache.Count; i++)
                RemoveMethodCache[i].Invoke(ntt, false);
        }
        public static void SaveComponents(string path) => Parallel.For(0, SaveMethodCache.Count, i => SaveMethodCache[i].Invoke(path));
    }
}