namespace MagnumOpus.Helpers
{
    public class StateMachine<T> where T : Enum
    {
        private readonly Dictionary<T, Dictionary<T, Action>> transitions;
        public T CurrentState { get; private set; }

        public StateMachine(T initialState)
        {
            CurrentState = initialState;
            transitions = [];
        }

        public void AddTransition(T fromState, T toState, Action action)
        {
            if (!transitions.ContainsKey(fromState))
                transitions[fromState] = [];
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
