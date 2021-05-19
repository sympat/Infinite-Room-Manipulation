using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class FiniteStateMachine<TState, TInput> 
    where TState : IComparable
    where TInput : IComparable
{
    Dictionary<TState, State> states;
    Dictionary<TState, Dictionary<TInput, Transition<TState>>> transitions;
    TState currentState;

    // public TState CurrentState {
    //     get { return currentState; }
    //     private set {
            
    //     }
    // }

    private event Action<TState> OnStateEnter;
    private event Action<TState> OnStateExit;
    private event Action<TState, TState> OnStateChange;
    private event Action<TInput> OnInput;

    public FiniteStateMachine(params TState[] states)
    {
        if (states.Length < 1) { throw new ArgumentException("A FiniteStateMachine needs at least 1 state", "states"); }

        transitions = new Dictionary<TState, Dictionary<TInput, Transition<TState>>>();
        this.states = new Dictionary<TState, State>();

        foreach (var value in states)
        {
            this.states.Add(value, new State());
            transitions.Add(value, new Dictionary<TInput, Transition<TState>>());
        }
    }

    public FiniteStateMachine<TState, TInput> AddStateStart(TState state, params Action[] onStarts) {
        foreach(var onStart in onStarts) 
            states[state].OnStart += onStart;

        return this;
    }

    public FiniteStateMachine<TState, TInput> AddStateEnd(TState state, params Action[] onEnds) {
        foreach(var onEnd in onEnds) 
            states[state].OnStart += onEnd;

        return this;      
    }

    public FiniteStateMachine<TState, TInput> AddTransition(TState from, TState to, TInput input, params Action[] onProcesses)
    {
        if (!states.ContainsKey(from)) { throw new ArgumentException("unknown state", "from"); }
        if (!states.ContainsKey(to)) { throw new ArgumentException("unknown state", "to"); }

        // add the transition to the db (new it if it does not exist)
        if(!transitions[from].ContainsKey(input))
            transitions[from].Add(input, null);

        transitions[from][input] = transitions[from][input] ?? new Transition<TState>(from, to);

        foreach(var onProcess in onProcesses) 
            transitions[from][input].OnProcess += onProcess;

        return this;
    }

    public FiniteStateMachine<TState, TInput> AddTransition(TState self, TInput input, params Action[] onProcesses)
    {
        if (!states.ContainsKey(self)) { throw new ArgumentException("unknown state", "self"); }

        // add the transition to the db (new it if it does not exist)
        if(!transitions[self].ContainsKey(input))
            transitions[self].Add(input, null);

        transitions[self][input] = transitions[self][input] ?? new Transition<TState>(self, self);
        
        foreach(var onProcess in onProcesses) 
            transitions[self][input].OnProcess += onProcess;

        return this;
    }

    public void Begin(TState firstState)
    {
        if (firstState == null) { throw new ArgumentNullException("firstState"); }
        if (!states.ContainsKey(firstState)) { throw new ArgumentException("unknown state", "firstState"); }

        currentState = firstState;
        if(OnStateEnter != null) OnStateEnter(currentState);
        states[currentState].Start();
    }

    public void Processing(TInput input)
    {
        if(OnInput != null) OnInput(input);

        if (transitions[currentState].ContainsKey(input))
        {
            var currentTransition = transitions[currentState][input];
            var nextState = currentTransition.ToState;

            if(nextState.Equals(currentState)) {
                currentTransition.Process();
            }
            else {

                if(OnStateExit != null) OnStateExit(currentState);
                states[currentState].End();

                if(OnStateChange != null) OnStateChange(currentState, nextState);
                currentTransition.Process();

                currentState = nextState;
                if(OnStateEnter != null) OnStateEnter(currentState);
                states[currentState].Start();

            }
        }
    }

    /// <summary>
    /// Adds a handler for entry into the given state
    /// </summary>
    /// <param name="state">The state to handle entry for</param>
    /// <param name="handler">The handler</param>
    /// <returns>The instance of FiniteStatemachine to comply with fluent interface pattern</returns>
    public FiniteStateMachine<TState, TInput> OnEnter(TState state, Action handler)
    {
        if (state == null) { throw new ArgumentNullException("state"); }
        if (handler == null) { throw new ArgumentNullException("handler"); }
        if (!states.ContainsKey(state)) { throw new ArgumentException("unknown state", "state"); }

        OnStateEnter += enteredState =>
        {
            if (enteredState.Equals(state))
            {
                handler();
            }
        };

        return this;
    }

    /// <summary>
    /// Adds a handler for exiting from the given state
    /// </summary>
    /// <param name="state">The state to handle exit from</param>
    /// <param name="handler">The handler</param>
    /// <returns>The instance of FiniteStatemachine to comply with fluent interface pattern</returns>
    public FiniteStateMachine<TState, TInput> OnExit(TState state, Action handler)
    {
        if (state == null) { throw new ArgumentNullException("state"); }
        if (handler == null) { throw new ArgumentNullException("handler"); }
        if (!states.ContainsKey(state)) { throw new ArgumentException("unknown state", "state"); }

        OnStateExit += exitedState =>
        {
            if (exitedState.Equals(state))
            {
                handler();
            }
        };
        return this;
    }

    /// <summary>
    /// Adds a handler for when the given from state changes to the given to state. The handler is called after transition, before OnEnter to the new state
    /// </summary>
    /// <param name="from">The from state</param>
    /// <param name="to">The to state</param>
    /// <param name="handler">The handler</param>
    /// <returns>The instance of FiniteStatemachine to comply with fluent interface pattern</returns>
    public FiniteStateMachine<TState, TInput> OnChange(TState from, TState to, Action handler)
    {
        if (from == null) { throw new ArgumentNullException("from"); }
        if (to == null) { throw new ArgumentNullException("to"); }
        if (!states.ContainsKey(from)) { throw new ArgumentException("unknown state", "from"); }
        if (!states.ContainsKey(to)) { throw new ArgumentException("unknown state", "to"); }
        if (handler == null) { throw new ArgumentNullException("handler"); }

        OnStateChange += (fromState, toState) =>
        {
            if (fromState.Equals(from) &&
                toState.Equals(to))
            {
                handler();
            }
        };

        return this;
    }

    public FiniteStateMachine<TState, TInput> OnEachInput(Action<TInput> handler) {
        if (handler == null) { throw new ArgumentNullException("handler"); }

        OnInput += handler;

        return this;
    }

    public FiniteStateMachine<TState, TInput> OnEnter(Action<TState> handler)
    {
        if (handler == null) { throw new ArgumentNullException("handler"); }

        OnStateEnter += handler;

        return this;
    }

    public FiniteStateMachine<TState, TInput> OnExit(Action<TState> handler)
    {
        if (handler == null) { throw new ArgumentNullException("handler"); }

        OnStateExit += handler;

        return this;
    }

    /// <summary>
    /// Adds a handler for any state change. The handler is called after transition, before OnEnter to the new state
    /// </summary>
    /// <param name="handler">A handler that provides the previous and new state</param>
    /// <returns>The instance of FiniteStatemachine to comply with fluent interface pattern</returns>
    public FiniteStateMachine<TState, TInput> OnChange(Action<TState, TState> handler)
    {
        if (handler == null) { throw new ArgumentNullException("handler"); }

        OnStateChange += handler;

        return this;
    }

}

public class State {
    public event Action OnStart;
    public event Action OnEnd;

    public void Start() {
        if(OnStart != null) OnStart();
    }

    public void End() {
        if(OnEnd != null) OnEnd();
    }

    public State() {}

    public State(Action newOnStart, Action newOnEnd) {
        OnStart += newOnStart;
        OnEnd += newOnEnd;
    }
}

public class Transition<TState> where TState : IComparable
{
    public TState FromState { get; private set; }
    public TState ToState { get; private set; }

    public event Action OnProcess;

    public Transition(TState from, TState to)
    {
        FromState = from;
        ToState = to;
    }

    public void Process() {
        if(OnProcess != null) OnProcess();
    }
}
