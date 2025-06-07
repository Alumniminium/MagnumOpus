using NttECS.ECS;

namespace MagnumOpus.AOGP
{
    public abstract class GOAPAction
    {
        public abstract int Cost { get; set; }
        public abstract bool PreconditionsFulfilled(in NTT ntt);
        public abstract void Execute(in NTT ntt);
        
        public virtual bool CanExecute(WorldState state, in NTT ntt)
        {
            return PreconditionsFulfilled(ntt);
        }
        
        public virtual WorldState PredictWorldState(WorldState currentState, in NTT ntt)
        {
            return new WorldState(currentState);
        }
        
        public virtual float CalculateCost(in NTT ntt)
        {
            return Cost;
        }
    }
}