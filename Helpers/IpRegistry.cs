using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IpRegistry
    {
        private static readonly Dictionary<string, NTT> IpToEntity = [];
        private static readonly Dictionary<NTT, string> EntityToIp = [];

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

        public static (bool found, string ip) GetIp(in NTT ntt) => EntityToIp.TryGetValue(ntt, out var ip) ? ((bool found, string ip))(true, ip) : ((bool found, string ip))(false, string.Empty);
        public static (bool found, NTT ntt) GetEntity(string ip) => IpToEntity.TryGetValue(ip, out var ntt) ? ((bool found, NTT ntt))(true, ntt) : ((bool found, NTT ntt))(false, default);
    }
}