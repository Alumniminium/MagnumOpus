using NttECS.ECS;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Registry for mapping entity names to entities and vice versa for name-based entity lookups.
    /// Maintains bidirectional mapping between entity names and their associated entities for player/NPC identification.
    /// </summary>
    public static class NameRegistry
    {
        private static readonly Dictionary<string, NTT> Name2Ntt = [];
        private static readonly Dictionary<NTT, string> Ntt2Name = [];

        /// <summary>
        /// Registers an entity with its associated name, removing any previous mappings.
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <param name="name">Name to associate with the entity</param>
        public static void Register(in NTT entity, string name)
        {
            if (Name2Ntt.TryGetValue(name, out var value))
            {
                Ntt2Name.Remove(value);
                Name2Ntt.Remove(name);
            }

            Name2Ntt.Add(name, entity);
            Ntt2Name.Add(entity, name);
        }

        /// <summary>
        /// Retrieves the name associated with an entity.
        /// </summary>
        /// <param name="ntt">Entity to lookup</param>
        /// <returns>Tuple containing success flag and entity name</returns>
        public static (bool found, string name) GetName(in NTT ntt) => Ntt2Name.TryGetValue(ntt, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        
        /// <summary>
        /// Retrieves the entity associated with a name.
        /// </summary>
        /// <param name="name">Name to lookup</param>
        /// <returns>Tuple containing success flag and entity</returns>
        public static (bool found, NTT ntt) GetEntity(string name) => Name2Ntt.TryGetValue(name, out var ntt) ? ((bool found, NTT ntt))(true, ntt) : ((bool found, NTT ntt))(false, default);
    }
}