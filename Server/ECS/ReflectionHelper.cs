using System.Reflection;

namespace MagnumOpus.ECS
{
    /// <summary>
    /// Reflection-based utility for performing type-safe operations on all component types.
    /// Pre-compiles method delegates for efficient execution of component operations across all registered types.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>Cached delegates for component removal operations</summary>
        private static readonly Dictionary<Type, Action<NTT, bool>> RemoveCache = [];
        /// <summary>Cached delegates for component save operations</summary>
        private static readonly Dictionary<Type, Action<string>> SaveCache = [];
        /// <summary>Cached delegates for component load operations</summary>
        private static readonly Dictionary<Type, Action<string>> LoadCache = [];
        /// <summary>Cached delegates for component ownership transfer operations</summary>
        private static readonly Dictionary<Type, Action<NTT, NTT>> ChangeOwnerCache = [];

        /// <summary>
        /// Initializes reflection helper by discovering and caching component operation delegates.
        /// </summary>
        static ReflectionHelper() => LoadMethods();

        /// <summary>
        /// Discovers all component types and pre-compiles operation delegates for efficient execution.
        /// </summary>
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

        /// <summary>
        /// Removes a component of the specified type from an entity using cached reflection delegates.
        /// </summary>
        /// <typeparam name="T">Component type to remove</typeparam>
        /// <param name="ntt">Entity to remove component from</param>
        public static void Remove<T>(NTT ntt)
        {
            if (!RemoveCache.TryGetValue(typeof(T), out var method))
                return;
            method.Invoke(ntt, true);
        }
        /// <summary>
        /// Transfers ownership of all components from one entity to another in parallel.
        /// </summary>
        /// <param name="from">Source entity to transfer components from</param>
        /// <param name="to">Target entity to transfer components to</param>
        public static void ChangeOwner(NTT from, NTT to) => Parallel.ForEach(ChangeOwnerCache.Values, method => method.Invoke(from, to));
        
        /// <summary>
        /// Removes all components from an entity without notifying systems (for recycling).
        /// </summary>
        /// <param name="ntt">Entity to recycle components from</param>
        public static void RecycleComponents(NTT ntt) => Parallel.ForEach(RemoveCache.Values, method => method.Invoke(ntt, false));
        
        /// <summary>
        /// Saves all component types to disk in parallel for server persistence.
        /// </summary>
        /// <param name="path">Directory path to save component data</param>
        public static void SaveComponents(string path) => Parallel.ForEach(SaveCache.Values, method => method.Invoke(path));
        
        /// <summary>
        /// Loads all component types from disk in parallel for server startup.
        /// </summary>
        /// <param name="path">Directory path to load component data from</param>
        public static void LoadComponents(string path) => Parallel.ForEach(LoadCache.Values, method => method.Invoke(path));
    }
}
