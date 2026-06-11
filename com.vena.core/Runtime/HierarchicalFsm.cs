// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Vena
{
    /// <summary>
    /// 有限状态机-状态抽象
    /// </summary>
    public interface IFSMState
    {
        void Init(IFSMActor actor);
        
        void TransitEnter(IFSMState from);
        
        void StateEnter();
        
        void Tick(float time, float deltaTime);
        
        void TransitExit(IFSMState to);
        
        void StateExit();
    }
    
    /// <summary>
    /// 有限状态机-状态的执行者
    /// </summary>
    public interface IFSMActor
    {
        void OnFsMachineEnterState(IFSMState state);
        
        void OnFsMachineExitState(IFSMState state);
    }
    
    /// <summary>
    /// 有限状态机-状态机
    /// </summary>
    public interface IFSMachine
    {
        void Start(int stateIndex);

        void Stop();
        
        bool Running { get; }
        
        void Tick(float time, float deltaTime);
        
        bool Transiting { get; }
        
        IFSMState Current { get; }
        
#pragma warning disable 693
        T GetState<T>() where T : class, IFSMState;

        bool SwitchState<T>( ) where T : class, IFSMState;

        bool InState<T>() where T : class, IFSMState;
        
        bool InStateTrans <T>() where T : class, IFSMTransition;
        
#pragma warning restore 693
    }
    
    public interface IFSMComplex : IFSMState
    {
        bool Running { get; }
        
        bool Transiting { get; }
        
        IFSMState Current { get; }

#pragma warning disable 693
        T GetState<T>() where T : class, IFSMState;

        bool SwitchState<T>( ) where T : class, IFSMState;

        bool InState<T>() where T : class, IFSMState;
        
        bool InStateTrans <T>() where T : class, IFSMTransition;
#pragma warning restore 693
    }
    
    /// <summary>
    /// 有限状态机-状态转换条件
    /// </summary>
    public interface IFSMTransition
    {
        Type First { get; }
        
        Type Second { get; }

        //check condition
        bool IsSatisfy(IFSMActor actor, IFSMState current, IFSMState next);
        
        bool Transition(float time, float deltaTime);
        
        void TransitStart(IFSMActor actor, IFSMState current,  IFSMState next);
        
        void TransitEnd();
    }

    /// <summary>
    /// 有限状态机-状态抽象模板
    /// </summary>
    public abstract class FSMState<TActor> : IFSMState where TActor : class, IFSMActor
    {
        public void Init(IFSMActor actor) { OnInitActor(actor as TActor); }
        
        public abstract void TransitEnter(IFSMState from);
        
        public abstract void StateEnter();

        public abstract void Tick(float time, float deltaTime);
        
        public abstract void TransitExit(IFSMState to);
        
        public abstract void StateExit();

        protected abstract void OnInitActor(TActor actor);
    }
    
    /// <summary>
    /// 有限状态机-状态转换条件 模板类
    /// 并标记转换的两个状态类型
    /// </summary>
    public abstract class FSMTransition<TActor,TState1,TState2> : IFSMTransition 
        where TActor : class, IFSMActor where TState1 : class, IFSMState where TState2 : class, IFSMState
    {
        public Type First => typeof(TState1);
        
        public Type Second => typeof(TState2);

        protected TActor Actor { private set; get; }
        
        protected TState1 CurrentState { private set; get; }
        
        protected TState2 NextState { private set; get; }
        
        public bool IsSatisfy(IFSMActor actor, IFSMState current, IFSMState next)
        {
            if (actor is TActor tActor && current is TState1 state1 && next is TState2 state2)
            {
                return CheckCondition(tActor, state1, state2);
            }
            
            return false;
        }
        
        public bool Transition(float time, float deltaTime)
        {
            return OnTransition(time, deltaTime);
        }

        public void TransitStart(IFSMActor actor, IFSMState current,  IFSMState next)
        {
            Actor = actor as TActor; 
            CurrentState = current as TState1; 
            NextState = next as TState2;
            OnTransitStart();
        }
        
        public void TransitEnd()
        {
            OnTransitEnd();
        }
        
        protected abstract bool CheckCondition(TActor actor, TState1 current, TState2 next);
        
        protected abstract bool OnTransition(float time, float deltaTime);
        
        protected abstract void OnTransitStart();
        
        protected abstract void OnTransitEnd();
        protected float LimitTime = 20f;
    }

    /// <summary>
    /// 有限状态机-状态机 模板类
    /// </summary>
    public class FSMStateMachine<T> : IFSMachine where T : class, IFSMActor
    {
        private IFSMState[] _states;

        private IFSMState _currentState;

        private Dictionary<Type, List<IFSMTransition>> _transitionMap;

        private Dictionary<Type, IFSMState> _stateMap;

        private IFSMTransition _currentTransition;

        private List<IFSMTransition> _currentStateTransitions;

        public bool Running { get; private set; } = false;

        public bool Transiting => _currentTransition != null;

        public T actor { get; private set; }

        public FSMStateMachine(T actor)
        {
            this.actor = actor;
        }

        public IFSMState Current => _currentState;

        public IFSMTransition CurrentTransition => _currentTransition;

        public IFSMState GetNextState()
        {
            if (Transiting &&_stateMap.TryGetValue(_currentTransition.Second, out var newState))
            {
                return newState;
            }
            return null;
        }

        public IFSMState[] states => _states;
        
        public void Init(IFSMState[] stateArray, IFSMTransition[] transitionArray = null)
        {
            if (null != _states) return;

            if (0 >= stateArray.Length)
                throw new Exception("FSMStateMachine.Init stateArray.Length <= 0");

            if (_transitionMap != null)
                throw new Exception("FSMStateMachine.Init _transitionMap != null");

            _stateMap = new Dictionary<Type, IFSMState>();
            _states = stateArray;
            
            foreach (var state in stateArray)
                _stateMap.Add(state.GetType(), state);
            
            _transitionMap = new Dictionary<Type, List<IFSMTransition>>();
            if (null != transitionArray)
            {
                foreach (var transition in transitionArray)
                {
                    if (!_transitionMap.TryGetValue(transition.First, out var toTransitList))
                    {
                        toTransitList = new List<IFSMTransition>();
                        _transitionMap.Add(transition.First, toTransitList);
                    }

                    if (toTransitList.Contains(transition))
                        throw new Exception($"Init transitionArray contains same {transition.First} -> {transition.Second}");
                    toTransitList.Add(transition);
                }
            }
            
            foreach (var state in _states) state.Init(actor);
            
            // init default state
            _currentState = null;
            _currentTransition = null;
            Running = false;
        }
        
        public void Start(int defaultStateIndex = 0)
        {
            if (Running ) return;
            
            if(null == _transitionMap)
                throw new Exception("FSMStateMachine.Start() _transitionMap == null");
            
            if(null == _states || _states.Length <= 0)
                throw new Exception("FSMStateMachine.Start() _states == null");
            
            if (defaultStateIndex < 0 || defaultStateIndex >= _states.Length)
                throw new ArgumentOutOfRangeException($"FSMStateMachine.Start() defaultStateIndex[{defaultStateIndex}] out of range !!");
            
            Running = true;
            _currentTransition = null;
            SetState(_states[defaultStateIndex]);
        }
        
        public void Stop()
        {
            if (!Running) return;
            Running = false;
            SetState(null);
        }

        public void Tick(float time, float deltaTime)
        {
            if (!Running) return;
            
            // tick transiting
            if (null != _currentTransition)
            {
                if (_currentTransition.Transition(time, deltaTime))
                {
                    _currentTransition.TransitEnd();
                    _stateMap.TryGetValue(_currentTransition.Second, out var newState);
                    _currentTransition = null;
                    
                    //DebugSystem.Assert(LogCategory.GameLogic, newState != null);
                    _SwitchState(newState);
                }
            }
            
            // tick transitions
            if (null == _currentTransition && null != _currentStateTransitions)
            {
                int length = _currentStateTransitions.Count;
                for (int i = 0; i < length; i++)
                {
                    var transition = _currentStateTransitions[i];
                    
                    // if transit condition satisfy, active this transition
                    IFSMState nextState = _stateMap[transition.Second];
                    if (transition.IsSatisfy(actor, _currentState, nextState))
                    {
                        _currentTransition = transition;
                        _currentTransition.TransitStart(actor, _currentState, nextState);
                        _currentState.TransitExit(nextState);
                        nextState.TransitEnter(_currentState);
                        break;
                    }
                }
            }

            // if not transiting, tick current state
            if (null == _currentTransition)
            {
                _currentState.Tick(time, deltaTime);
            }
        }
        
        private bool _SwitchState(IFSMState newState)
        {
            if (_currentState == newState)  return false;
            
            var oldState = _currentState;
            if (oldState != null)
            {
                oldState.StateExit();
                actor.OnFsMachineExitState(oldState);
            }
            
            _currentState = null;
            _currentState = newState;
            if (_currentState != null)
            {
                _transitionMap.TryGetValue(_currentState.GetType(), out _currentStateTransitions);
                _currentState.StateEnter();
                actor.OnFsMachineEnterState(_currentState);
                return true;
            }
            
            return false;
        }
        
        public bool SetState(IFSMState newState)
        {
            var oldState = _currentState;
            if (newState == oldState) return false;
            
            if (null != _currentTransition)
            {
                _currentTransition.TransitEnd();
                _currentTransition = null;
            }
            
            oldState?.TransitExit(newState);
            newState?.TransitEnter(oldState);
            return _SwitchState(newState);
        }
        
        public bool SwitchState<TState>() where  TState : class, IFSMState
        {
            var newState = GetState<TState>();
            if (null == newState)
            {
                throw new Exception($"newState[{typeof(TState).Name}] == null !!");
            }
            
            return SetState(newState);
        }
        
        public bool SwitchState(int stateIndex)
        {
            var newState = _states[stateIndex];
            if (null == newState)
            {
                throw new Exception($"index of {stateIndex} == null !!");
            }
            return SetState(newState);
        }
        
        public TState GetState<TState>() where TState : class, IFSMState
        {
            if(_stateMap.TryGetValue(typeof(TState), out var state))
                return state as TState;

            foreach (var state1 in _stateMap.Values)
            {
                if (state1 is TState tState)
                    return tState;
            }
            
            return null;
        }
        
        public bool InState<TState>() where TState : class, IFSMState
        {
            return _currentState is TState;
        }
        
        public bool InStateTrans<TState>() where TState : class, IFSMTransition
        {
            return _currentTransition is TState;
        }
    }
    
    /// <summary>
    /// 复合状态机- 使状态内部包含状态机
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FSMComplex<T> : IFSMComplex where T : class, IFSMActor
    {
        private T _actor;
        public readonly FSMStateMachine<T> fsMachine;
        
        public T actor => _actor;
        
        public bool Running => fsMachine.Running;
        public bool Transiting => fsMachine.Transiting;
        
        public IFSMState Current => fsMachine.Current;
        
        public IFSMTransition CurrentTransition => fsMachine.CurrentTransition;
        
        public T1 GetState<T1>() where T1 : class, IFSMState => fsMachine.GetState<T1>();

        public bool SwitchState<T1>() where T1 : class, IFSMState => fsMachine.SwitchState<T1>();
    
        public bool InState<T1>() where T1 : class, IFSMState => fsMachine.InState<T1>();

        public bool InStateTrans<T1>() where T1 : class, IFSMTransition => fsMachine.InStateTrans<T1>();

        protected FSMComplex(FSMStateMachine<T> machine)
        {
            fsMachine = machine;
        }
        
        void IFSMState.Init(IFSMActor actor)
        {
            _actor = actor as T;
            OnInit();
        }
        
        protected abstract void OnInit();
        
        void IFSMState.TransitEnter(IFSMState from)
        {
            OnTransitEnter(from);
        }
        
        protected abstract void OnTransitEnter(IFSMState from);
        
        void IFSMState.StateEnter()
        {
            OnStateEnter();
        }
        
        protected abstract void OnStateEnter();

        void IFSMState.Tick(float time, float deltaTime)
        {
            OnTick(time, deltaTime);
            fsMachine.Tick(time, deltaTime);
        }
        
        protected abstract void OnTick(float time, float deltaTime);
        
        void IFSMState.TransitExit(IFSMState to)
        {
            OnTransitExit(to);
        }
        
        protected abstract void OnTransitExit(IFSMState to);
        
        void IFSMState.StateExit()
        {
            OnStateExit();
        }
        
        protected abstract void OnStateExit();
    }
}