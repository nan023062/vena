
using System;
using System.Collections.Generic;

namespace Vena
{
    public interface ITaskClip
    {
        float duration { get; }

        void Start(ITaskContext context);

        bool Tick(float deltaTime);

        void Finish();
        
        void Replay(ITaskContext context);
    }
    
    public abstract class TaskNode : ITaskClip
    {
        private static readonly List<int> _emptyList = new List<int>();
        private ITaskContext _context;
        private readonly Dictionary<int, TaskEvent> _timeEvents = new Dictionary<int, TaskEvent>();
        
        protected ITaskContext Context => _context;
        
        public float elapsedTime { private set; get; }

        public int frame { private set; get; }

        public bool isPlaying { get; private set; } = false;

        protected TArgs GetArgs<TArgs>() where TArgs : class, ITaskContext
        {
            return _context as TArgs;
        }

        public abstract float duration { get; }

        public abstract bool CanInterrupted();
        
        public TaskNode AddEvent(TaskEvent eventClip)
        {
            _timeEvents.Add(eventClip.eventId, eventClip);
            return this;
        }
        
        public TaskNode ExecuteEvent(int eventId)
        {
            if (_timeEvents.Remove(eventId))
            {
                if(_context is IHasLogicEventClip eventAction)
                    eventAction.ExecuteEvent(eventId);
            }
            return this;
        }
        
        public void Start(ITaskContext context)
        {
            if (!isPlaying)
            {
                _context = context;
                try
                {
                    Log("Start");
                    OnStart();
                    elapsedTime = 0f;
                    frame = 0;
                    isPlaying = true;
                }
                catch (Exception e)
                {
                    string startType = context.GetType().Name;
                    throw new Exception($"{GetType().Name}.Start({startType}) : error = {e.Message}, stack = {e.StackTrace}!");
                }
            }
            else
            {
                string runningType = _context.GetType().Name;
                string startType = context.GetType().Name;
                throw new Exception($"{GetType().Name}.Start({startType}) error， is already {runningType} running !");
            }
        }

        public bool Tick(float deltaTime)
        {
            if (!isPlaying) return true;

            // update time
            elapsedTime += deltaTime;
            frame++;

            // execute event
            if (_timeEvents.Count > 0)
            {
                try
                {
                    _emptyList.Clear();
                    foreach (var keyValue in _timeEvents)
                    {
                        float time = keyValue.Value.time;
                        if (time >=0f && time <= elapsedTime)
                            _emptyList.Add(keyValue.Key);
                    }

                    if (_emptyList.Count > 0)
                    {
                        foreach (int eventId in _emptyList)
                            ExecuteEvent(eventId);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"{GetType().Name}.execute event : error = {e.Message}, stack = {e.StackTrace}!");
                }
                finally
                {
                    _emptyList.Clear();
                }
            }
            
            try
            {
                // tick clip 
                return OnTick(elapsedTime, deltaTime);
            }
            catch (Exception e)
            {
                throw new Exception($"{GetType().Name}.Tick() : error = {e.Message}, stack = {e.StackTrace}!");
            }
        }

        public void Finish()
        {
            if (isPlaying)
            {
                try
                {
                    Log("Finish");
                    OnFinish();
                }
                catch (Exception e)
                {
                    throw new Exception($"{GetType().Name}.Finish() : error = {e.Message}, stack = {e.StackTrace}!");
                }
                finally
                {
                    Reset();
                }
            }
        }
        
        public void Replay(ITaskContext context)
        {
            _context = context;
            try
            {
                Log("Replay");
                OnReplay();
            }
            catch (Exception e)
            {
                Finish();
                
                throw new Exception($"{GetType().Name}.Replay() : error = {e.Message}, stack = {e.StackTrace}!");
            }
        }
        
        private void Reset()
        {
            // execute require event
            if (_timeEvents.Count > 0)
            {
                try
                {
                    _emptyList.Clear();
                    foreach (var keyValue in _timeEvents)
                        if(keyValue.Value.type == EventType.Required)
                            _emptyList.Add(keyValue.Key);
                    
                    if (_emptyList.Count > 0)
                    {
                        foreach (int eventId in _emptyList)
                            ExecuteEvent(eventId);
                    }
                    _timeEvents.Clear();
                }
                catch (Exception e)
                {
                    throw new Exception($"{GetType().Name}.execute require event : error = {e.Message}, stack = {e.StackTrace}!");
                }
            }
            
            // reset
            OnResetData();
            elapsedTime = 0f;
            frame = 0;
            isPlaying = false;
            _context = null;
        }

        protected void LogElapsedTime(int tag)
        {
            // if (tag == -1)
            //     DebugSystem.LogProduct(LogCategory.Framework,
            //         $"[{GetType().Name}] < time out >  frame = {frame}, time = {elapsedTime}!!");
            // else
            //     DebugSystem.LogWarning(LogCategory.Framework,
            //         $"[{GetType().Name}] < {tag} >  frame = {frame}, time = {elapsedTime}!!");
        }

        protected void Log(string func)
        {
            //DebugSystem.LogWarning(LogCategory.All, $"### {GetType().Name}.{func}() : {Context.GetType().Name}!");
        }

        #region abstract funcs
        
        protected abstract void OnStart();

        protected abstract bool OnTick(float elapsedTime, float deltaTime);
        
        protected abstract void OnFinish();
        
        protected abstract void OnReplay();

        protected abstract void OnResetData();

        #endregion
    }

    public abstract class TaskClip : TaskNode
    {
        protected abstract float timeOut { get; }

        private float _safe_time_check = 20f;
        
        public sealed override float duration => timeOut;
        
        protected void ResetSafeTime(float safe_time)
        {
            _safe_time_check = safe_time;
        }

        public override bool CanInterrupted() => true;
        
        protected sealed override void OnStart()
        {
            ResetSafeTime(timeOut);

            OnStartSafe();
        }

        protected abstract void OnStartSafe();

        protected sealed override bool OnTick(float time, float deltaTime)
        {
            _safe_time_check -= deltaTime;

            if (_safe_time_check <= 0)
            {
                LogElapsedTime(-1);
                return true;
            }

            return OnTickSafe(time, deltaTime);
        }

        protected abstract bool OnTickSafe(float time, float deltaTime);

        protected override void OnReplay()
        {
        }
    }
}