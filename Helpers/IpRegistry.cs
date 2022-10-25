using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IpRegistry
    {
        private static readonly Dictionary<string, PixelEntity> NameToEntity = new ();
        private static readonly Dictionary<PixelEntity, string> EntityToName = new ();

        public static void Register(PixelEntity entity, string ip)
        {
            if (NameToEntity.TryGetValue(ip, out var value))
            {
                EntityToName.Remove(value);
                NameToEntity.Remove(ip);
            }

            NameToEntity.Add(ip, entity);
            EntityToName.Add(entity, ip);
        }

        public static (bool found, string name) GetIp(PixelEntity entity)
        {
            return EntityToName.TryGetValue(entity, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        }
        public static (bool found, PixelEntity ntt) GetEntity(string ip)
        {
            return NameToEntity.TryGetValue(ip, out var entity) ? ((bool found, PixelEntity ntt))(true, entity) : ((bool found, PixelEntity ntt))(false, default);
        }
    }
}