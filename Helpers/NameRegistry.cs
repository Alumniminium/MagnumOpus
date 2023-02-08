using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class NameRegistry
    {
        private static readonly Dictionary<string, NTT> NameToEntity = new ();
        private static readonly Dictionary<NTT, string> EntityToName = new ();

        public static void Register(in NTT entity, string name)
        {
            if (NameToEntity.TryGetValue(name, out var value))
            {
                EntityToName.Remove(value);
                NameToEntity.Remove(name);
            }

            NameToEntity.Add(name, entity);
            EntityToName.Add(entity, name);
        }

        public static (bool found, string name) GetName(in NTT entity)
        {
            return EntityToName.TryGetValue(entity, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        }
        public static (bool found, NTT ntt) GetEntity(string name)
        {
            return NameToEntity.TryGetValue(name, out var entity) ? ((bool found, NTT ntt))(true, entity) : ((bool found, NTT ntt))(false, default);
        }
    }
}