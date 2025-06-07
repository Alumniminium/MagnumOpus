using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Registry for mapping IP addresses to entities and vice versa for connection tracking.
    /// Maintains bidirectional mapping between client IP addresses and their associated entities.
    /// </summary>
    public static class IpRegistry
    {
        private static readonly Dictionary<string, NTT> IpToEntity = [];
        private static readonly Dictionary<NTT, string> EntityToIp = [];

        /// <summary>
        /// Registers an entity with its associated IP address, removing any previous mappings.
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <param name="ip">IP address to associate with the entity</param>
        public static void Register(in NTT entity, string ip)
        {
            if (IpToEntity.TryGetValue(ip, out var value))
            {
                EntityToIp.Remove(value);
                IpToEntity.Remove(ip);
            }

            IpToEntity.Add(ip, entity);
            EntityToIp.Add(entity, ip);
        }

        /// <summary>
        /// Retrieves the IP address associated with an entity.
        /// </summary>
        /// <param name="ntt">Entity to lookup</param>
        /// <returns>Tuple containing success flag and IP address</returns>
        public static (bool found, string ip) GetIp(in NTT ntt) => EntityToIp.TryGetValue(ntt, out var ip) ? ((bool found, string ip))(true, ip) : ((bool found, string ip))(false, string.Empty);
        
        /// <summary>
        /// Retrieves the entity associated with an IP address.
        /// </summary>
        /// <param name="ip">IP address to lookup</param>
        /// <returns>Tuple containing success flag and entity</returns>
        public static (bool found, NTT ntt) GetEntity(string ip) => IpToEntity.TryGetValue(ip, out var ntt) ? ((bool found, NTT ntt))(true, ntt) : ((bool found, NTT ntt))(false, default);
    }
}