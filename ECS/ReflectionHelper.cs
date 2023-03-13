using System.Reflection;

namespace MagnumOpus.ECS
{
    public static class ReflectionHelper
    {
        private static readonly List<Action<NTT, bool>> RemoveMethodCache = new();
        private static readonly Dictionary<Type, Action<NTT, bool>> RemoveCache = new();

        private static readonly List<Action<string>> SaveMethodCache = new();
        private static readonly Dictionary<Type, Action<string>> SaveCache = new();
        private static readonly List<Action<string>> LoadMethodCache = new();
        private static readonly Dictionary<Type, Action<string>> LoadCache = new();
        private static readonly Dictionary<Type, Action<NTT, NTT>> ChangeOwnerCache = new();
        private static readonly List<Action<NTT, NTT>> ChangeOwnerMethodCache = new();
        static ReflectionHelper()
        {
            GetRemoveMethods();
            GetSaveMethods();
            GetLoadMethods();
            GetChangeOwnerMethods();
        }

        public static void GetRemoveMethods()
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
                RemoveCache.TryAdd(type, method);
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
        public static void GetLoadMethods()
        {
            var enumerable = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Select(t => new { t, l = t.GetCustomAttributes(typeof(SaveAttribute), true) })
                        .Where(t => t.l.Length > 0)
                        .Select(t => t.t)
                        .ToList();

            var methods = enumerable.Select(ct => (Action<string>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Load")!.CreateDelegate(typeof(Action<string>)));

            LoadMethodCache.AddRange(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = LoadMethodCache[i];
                LoadCache.Add(type, method);
            }
        }



        private static void GetChangeOwnerMethods()
        {
            var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Select(t => new { t, aList = t.GetCustomAttributes(typeof(ComponentAttribute), true) })
                            .Where(t1 => t1.aList.Length > 0)
                            .Select(t1 => t1.t);

            var enumerable = types as Type[] ?? types.ToArray();
            var methods = enumerable.Select(ct => (Action<NTT, NTT>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("ChangeOwner")!.CreateDelegate(typeof(Action<NTT, NTT>)));

            ChangeOwnerMethodCache.AddRange(methods);
            var componentTypes = new List<Type>(enumerable);

            for (var i = 0; i < componentTypes.Count; i++)
            {
                var type = componentTypes[i];
                var method = ChangeOwnerMethodCache[i];
                ChangeOwnerCache.TryAdd(type, method);
            }
        }

        public static void Remove<T>(NTT ntt)
        {
            if (!RemoveCache.TryGetValue(typeof(T), out var method))
                return;
            method.Invoke(ntt, true);
        }
        public static void ChangeOwner(NTT from, NTT to) => Parallel.For(0, ChangeOwnerCache.Count, i => ChangeOwnerMethodCache[i].Invoke(from, to));
        public static void RecycleComponents(NTT ntt) => Parallel.For(0, RemoveMethodCache.Count, i => RemoveMethodCache[i].Invoke(ntt, false));
        public static void SaveComponents(string path) => Parallel.For(0, SaveMethodCache.Count, i => SaveMethodCache[i].Invoke(path));
        public static void LoadComponents(string path) => Parallel.For(0, LoadMethodCache.Count, i => LoadMethodCache[i].Invoke(path));
    }
}