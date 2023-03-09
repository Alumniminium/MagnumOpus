using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class NameRegistry
    {
        private static readonly Dictionary<string, NTT> Name2Ntt = new();
        private static readonly Dictionary<NTT, string> Ntt2Name = new();

        public static void Register(in NTT entity, string name)
        {
            if (Name2Ntt.TryGetValue(name, out var value))
            {
                _ = Ntt2Name.Remove(value);
                _ = Name2Ntt.Remove(name);
            }

            Name2Ntt.Add(name, entity);
            Ntt2Name.Add(entity, name);
        }

        public static (bool found, string name) GetName(in NTT ntt) => Ntt2Name.TryGetValue(ntt, out var name) ? ((bool found, string name))(true, name) : ((bool found, string name))(false, string.Empty);
        public static (bool found, NTT ntt) GetEntity(string name) => Name2Ntt.TryGetValue(name, out var ntt) ? ((bool found, NTT ntt))(true, ntt) : ((bool found, NTT ntt))(false, default);
    }
}