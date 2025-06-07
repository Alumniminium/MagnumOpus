namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Generic state machine implementation with transition actions for managing entity states and behaviors.
    /// Provides type-safe state transitions with custom actions executed during state changes.
    /// </summary>
    /// <typeparam name="T">Enum type representing the possible states</typeparam>
    /// <param name="initialState">Starting state for the state machine</param>
    public class StateMachine<T>(T initialState) where T : Enum
    {
        private readonly Dictionary<T, Dictionary<T, Action>> transitions = [];
        
        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        public T CurrentState { get; private set; } = initialState;

        /// <summary>
        /// Adds a transition from one state to another with an action to execute during the transition.
        /// </summary>
        /// <param name="fromState">Source state for the transition</param>
        /// <param name="toState">Target state for the transition</param>
        /// <param name="action">Action to execute when transitioning</param>
        public void AddTransition(T fromState, T toState, Action action)
        {
            if (!transitions.ContainsKey(fromState))
                transitions[fromState] = [];
            transitions[fromState][toState] = action;
        }

        /// <summary>
        /// Attempts to transition to the specified next state, executing the associated action if a valid transition exists.
        /// </summary>
        /// <param name="nextState">State to transition to</param>
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
