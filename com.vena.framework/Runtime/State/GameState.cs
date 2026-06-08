using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vena.Framework
{
    internal interface IBasedStackGameState
    {
        void StateCreate();

        Queue<IGameStateJob> StateFocus();
        
        void StateUpdate(float time, float deltaTime);
        
        Queue<IGameStateJob> StateUnFocus();
        
        void StateDestroy();
    }
    
    public abstract class BasedStackGameState : IBasedStackGameState
    {
        readonly List<IModule> _models = new List<IModule>();
        
        readonly List<IModule> _modules = new List<IModule>();
        
        void IBasedStackGameState.StateCreate()
        {
            _models.Clear();
            
            _models.AddRange(GameWorld.RegisterModels(this));

            foreach (var module in _models)
            {
                try
                {
                    module.Start();
                }
                catch (Exception e)
                {
                    Debug.Error( $"{module} : {e}");
                }
            }
        }
        
        /// <summary>
        /// 状态进入栈顶时，激活所有的子模块
        /// </summary>
        /// <returns></returns>
        Queue<IGameStateJob> IBasedStackGameState.StateFocus()
        {
            _modules.Clear();
            
            _modules.AddRange(GameWorld.RegisterModules(this));
            
            var jobs = new Queue<IGameStateJob>();
                 
            // init modules jobs
            foreach (var module in _modules)
            {
                var moduleJob = new ModuleStartJob(module);
                
                jobs.Enqueue(moduleJob);
            }
                
            // focus state job
            jobs.Enqueue(new ActionJob(StateFocus,$"{GetType().Name}.Focus"));
            
            return jobs;
        }
        
        void IBasedStackGameState.StateUpdate(float time, float deltaTime)
        {
            StateUpdate(time, deltaTime);
        }
        
        Queue<IGameStateJob> IBasedStackGameState.StateUnFocus()
        {
            var jobs = new Queue<IGameStateJob>();
            
            // unfocus state job
            jobs.Enqueue(new ActionJob(StateUnFocus, $"{GetType().Name}.Unfocus"));
            
            // release modules jobs
            for (int i = _modules.Count - 1; i >= 0; i--)
            {
                var module = _modules[i];
                
                var moduleJob = new ModuleReleaseJob(module);
                
                jobs.Enqueue(moduleJob);
            }
            
            // unregister modules
            _modules.Clear();
            
            jobs.Enqueue(new ActionJob(() => 
                GameWorld.UnRegisterModules(this), $"{GetType().Name}.Destroy"));
            
            return jobs;
        }

        void IBasedStackGameState.StateDestroy()
        {
            StateDestroy();
            
            _modules.Clear();
            
            for (int i = _models.Count - 1; i >= 0; i--)
            {
                var module = _models[i];
                try
                {
                    module.Release();
                }
                catch (Exception e)
                {
                    Debug.Error( $"{module} : {e}");
                }
            }
 
            GameWorld.UnRegisterModels(this);
        }
        
        protected abstract void StateFocus();
        
        protected virtual void StateUpdate(float time, float deltaTime) { }
        
        protected abstract void StateUnFocus();
        
        protected abstract void StateDestroy();
    }

    /// <summary>
    /// 将整个游戏设计成栈状态机
    /// 这种状态机模型的关键优点是，每个状态都是独立的，且状态之间的转换关系非常清晰。
    /// 进入新状态时，旧状态被保存，如果需要，可以轻松地回到旧状态。
    /// 这种模型对于处理暂停菜单、设置选项、游戏过场和层叠的用户界面等非常有效。
    /// </summary>
    public abstract class BasedStackGameMachine : BasedStackGameState
    {
        private BasedStackGameState _current;
        
        private Transition _transition;
        
        private readonly Stack<BasedStackGameState> _stateStack = new Stack<BasedStackGameState>();
        
        protected BasedStackGameState current => _current;
        
        protected override void StateFocus()
        {
            _stateStack.Clear();
            
            _current = null;
        }
        
        protected override void StateUpdate(float time, float deltaTime)
        {
            if (null == _transition)
            {
                if (_current is IBasedStackGameState stackState)
                {
                    stackState.StateUpdate(time, deltaTime);
                }
            }
        }

        protected override void StateUnFocus()
        {
        }
        
        protected sealed override void StateDestroy()
        {
            if (null != _transition)
            {
                _transition.Dispose();
                _transition = null;
            }

            OnDestroy();
        }

        protected virtual void OnDestroy()
        {
            
        }
        
        #region job for game state
        
        /// <summary>
        /// enter
        /// </summary>
        /// <param name="enterState"></param>
        /// <param name="transition"></param>
        internal async Task Enter(BasedStackGameState enterState, IGameTransition transition)
        {
            if (_transition != null)
            {
                throw new Exception(" transition is running!  ");
            }
            
            _transition = new Transition(transition);
            BasedStackGameState prev = _current;
            IBasedStackGameState nextState = enterState;
            
            nextState.StateCreate();
            
            if (null != _current)
            {
                _stateStack.Push(_current);
                _current = null;
            }
            
            _current = enterState;
            
            // exit pre state && enter next state by transition
            var args = GetTransitJobs(prev, _current, false);
            await _transition.Run(args);
            _transition.Dispose();
            _transition = null;
        }

        /// <summary>
        /// exit
        /// </summary>
        internal async Task Exit(IGameTransition transition)
        {
            if (_transition != null)
            {
                throw new Exception(" transition is running!  ");
            }
            
            if (_current != null && _stateStack.Count > 0)
            {
                _transition = new Transition(transition);
                BasedStackGameState prev = _current;
                
                _current = _stateStack.Pop();
                
                // enter next state && exit pre state by transition
                var args = GetTransitJobs(prev, _current, true);
                await _transition.Run(args);
                _transition.Dispose();
                _transition = null;
            }
        }
        
        private TransitArgs GetTransitJobs(BasedStackGameState prev, BasedStackGameState next, bool exit)
        {
            var args = new TransitArgs(prev, next);
            
            // exit jobs
            if (null != prev)
            {
                // unfocus state job
                GetUnfocusJobs(prev, ref args);

                if (exit)
                {
                    GetExitJobs(prev, ref args);
                }
            }
            
            // focus state job
            if (null != next)
            {
                GetFocusJobs(next, ref args);
            }
            
            return args;
        }
        
        private void GetFocusJobs(IBasedStackGameState focus, ref TransitArgs jobs)
        {
            var jobQueue = focus.StateFocus();
            
            while (jobQueue.Count > 0)
            {
                var job = jobQueue.Dequeue();
                
                jobs.Jobs.Enqueue(job);
                
                jobs.TotalSeconds += job.estimatedSeconds;
            }
        }
        
        private void GetUnfocusJobs(IBasedStackGameState unfocus, ref TransitArgs jobs)
        {
            if (unfocus is BasedStackGameMachine stateMachine)
            {
                // exit sub state job
                IBasedStackGameState subState = stateMachine.current;
                
                if (null != subState)
                {
                    GetUnfocusJobs(subState, ref jobs);
                
                    GetExitJobs(subState, ref jobs);
                }
                
                // exit sub state stack job
                while (stateMachine._stateStack.Count > 0)
                {
                    subState = stateMachine._stateStack.Pop();
                    
                    GetExitJobs(subState, ref jobs);
                }
            }

            // unfocus state job
            var jobQueue = unfocus.StateUnFocus();
            
            while (jobQueue.Count > 0)
            {
                var job = jobQueue.Dequeue();
                
                jobs.Jobs.Enqueue(job);
                
                jobs.TotalSeconds += job.estimatedSeconds;
            }
        }
        
        private void GetExitJobs(IBasedStackGameState exit, ref TransitArgs jobs)
        {
            var exitJob = new ActionJob(exit.StateDestroy, $"{GetType().Name}.Destroy");
            
            jobs.Jobs.Enqueue(exitJob);
            
            jobs.TotalSeconds += exitJob.estimatedSeconds;
        }

        #endregion
    }
}