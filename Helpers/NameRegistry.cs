using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class NameRegistry
    {
        private static readonly Dictionary<string, PixelEntity> NameToEntity = new ();
        private static readonly Dictionary<PixelEntity, string> EntityToName = new ();

        public static void Register(in PixelEntity entity, string name)
        {
            if (NameToEntity.TryGetValue(name, out var value))
            {
                EntityToName.Remove(value);
                NameToEntity.Remove(name);
            }

            NameToEntity.Add(name, entity);
            EntityToName.Add(entity, name);
        }

        public static (bool found, string name) GetName(in PixelEntity entity)
        {
            return EntityToName.TryGetValue(entity, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        }
        public static (bool found, PixelEntity ntt) GetEntity(string name)
        {
            return NameToEntity.TryGetValue(name, out var entity) ? ((bool found, PixelEntity ntt))(true, entity) : ((bool found, PixelEntity ntt))(false, default);
        }
    }
}