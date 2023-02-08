using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IpRegistry
    {
        private static readonly Dictionary<string, NTT> NameToEntity = new ();
        private static readonly Dictionary<NTT, string> EntityToName = new ();

        public static void Register(in NTT entity, string ip)
        {
            if (NameToEntity.TryGetValue(ip, out var value))
            {
                EntityToName.Remove(value);
                NameToEntity.Remove(ip);
            }

            NameToEntity.Add(ip, entity);
            EntityToName.Add(entity, ip);
        }

        public static (bool found, string name) GetIp(in NTT entity)
        {
            return EntityToName.TryGetValue(entity, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        }
        public static (bool found, NTT ntt) GetEntity(string ip)
        {
            return NameToEntity.TryGetValue(ip, out var entity) ? ((bool found, NTT ntt))(true, entity) : ((bool found, NTT ntt))(false, default);
        }
    }
}