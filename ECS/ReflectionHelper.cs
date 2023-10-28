using System.Reflection;

namespace MagnumOpus.ECS
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, Action<NTT, bool>> RemoveCache = [];
        private static readonly Dictionary<Type, Action<string>> SaveCache = [];
        private static readonly Dictionary<Type, Action<string>> LoadCache = [];
        private static readonly Dictionary<Type, Action<NTT, NTT>> ChangeOwnerCache = [];

        static ReflectionHelper() => LoadMethods();

        private static void LoadMethods()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            var componentTypes = types
                .Where(t => t.GetCustomAttributes(typeof(ComponentAttribute), true).Length > 0)
                .ToList();

            foreach (var ct in componentTypes)
            {
                var removeMethod = (Action<NTT, bool>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Remove")!.CreateDelegate(typeof(Action<NTT, bool>));
                RemoveCache.Add(ct, removeMethod);

                var changeOwnerMethod = (Action<NTT, NTT>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("ChangeOwner")!.CreateDelegate(typeof(Action<NTT, NTT>));
                ChangeOwnerCache.Add(ct, changeOwnerMethod);

                var saveAttribute = ct.GetCustomAttribute<ComponentAttribute>();
                if (saveAttribute?.SaveEnabled ?? false)
                {
                    var saveMethod = (Action<string>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Save")!.CreateDelegate(typeof(Action<string>));
                    SaveCache.Add(ct, saveMethod);

                    var loadMethod = (Action<string>)typeof(SparseComponentStorage<>).MakeGenericType(ct).GetMethod("Load")!.CreateDelegate(typeof(Action<string>));
                    LoadCache.Add(ct, loadMethod);
                }
            }
        }

        public static void Remove<T>(NTT ntt)
        {
            if (!RemoveCache.TryGetValue(typeof(T), out var method))
                return;
            method.Invoke(ntt, true);
        }
        public static void ChangeOwner(NTT from, NTT to) => Parallel.ForEach(ChangeOwnerCache.Values, method => method.Invoke(from, to));
        public static void RecycleComponents(NTT ntt) => Parallel.ForEach(RemoveCache.Values, method => method.Invoke(ntt, false));
        public static void SaveComponents(string path) => Parallel.ForEach(SaveCache.Values, method => method.Invoke(path));
        public static void LoadComponents(string path) => Parallel.ForEach(LoadCache.Values, method => method.Invoke(path));
    }
}
