namespace MagnumOpus.Helpers
{
    public class StateMachine<T>
    {
        private readonly Dictionary<T, Dictionary<T, Action>> transitions;
        public T CurrentState { get; private set; }

        public StateMachine(T initialState)
        {
            CurrentState = initialState;
            transitions = new Dictionary<T, Dictionary<T, Action>>();
        }

        public void AddTransition(T fromState, T toState, Action action)
        {
            if (!transitions.ContainsKey(fromState))
                transitions[fromState] = new Dictionary<T, Action>();
            transitions[fromState][toState] = action;
        }

        public void MoveNext(T nextState)
        {
            if (transitions.TryGetValue(CurrentState, out var actions) && actions.TryGetValue(nextState, out var action))
            {
                action();
                CurrentState = nextState;
            }
        }
    }
}
