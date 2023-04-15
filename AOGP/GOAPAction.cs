using MagnumOpus.ECS;

namespace MagnumOpus.AOGP
{
    public abstract class GOAPAction
    {
        public abstract int Cost { get; set; }
        public abstract bool PreconditionsFulfilled(in NTT ntt);
        public abstract void Execute(in NTT ntt);
        public abstract void UpdateEffects(in NTT ntt);
    }
}