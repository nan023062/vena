using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vena
{
    public abstract class TaskTrack : TaskNode
    {
        // 0: not started, 1: playing, 2: done
        protected enum State
        {
            NotStarted,
            Playing,
            Done
        }

        protected class ClipWrapper
        {
            public float startTime;
            public float endTime;
            public State state;
            public TaskNode clip;

            public void SetState(State state, ITaskContext context)
            {
                if (this.state != state)
                {
                    this.state = state;
                    if (state == State.Playing)
                    {
                        try
                        {
                            clip.Start(context);
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"{clip.GetType().Name}.Play(), will not played! {e} !!");
                        }
                    }
                    else if (state == State.Done)
                    {
                        try
                        {
                            clip.Finish();
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"{clip.GetType().Name}.Stop() ! {e} !!");
                        }
                    }
                }
            }

            public bool Update(ITaskContext context, float elapsed, float deltaTime)
            {
                if (state == State.NotStarted)
                {
                    if (elapsed >= startTime)
                    {
                        SetState(State.Playing, context);
                    }
                }
                else if (state == State.Playing)
                {
                    try
                    {
                        if (clip.Tick(deltaTime))
                            SetState(State.Done, context);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"{clip.GetType().Name}.Update() ! {e} !!");
                    }
                }

                return state == State.Done;
            }
        }
    }
    
    public sealed class TaskSequence : TaskTrack
    {
        private Queue<ClipWrapper> _clips = new Queue<ClipWrapper>();
        private Queue<ClipWrapper> _finished = new Queue<ClipWrapper>();
        
        private float _duration;
        private ClipWrapper _segment;
        
        public override float duration => _duration;

        public override bool CanInterrupted() 
        {
            if(null == _segment) return true;
            return _segment.clip.CanInterrupted();
        }
        
        public void Enqueue(TaskNode clip)
        {
            if (clip == null) return;

            if (null != _segment)
                throw new Exception("Can't add playable when track is playing");

            ClipWrapper clipWrapper = new ClipWrapper()
            {
                startTime = _duration,
                endTime = _duration + clip.duration,
                clip = clip,
                state = State.NotStarted
            };
            _duration = clipWrapper.endTime;
            _clips.Enqueue(clipWrapper);
        }
        
        protected override void OnStart()
        {
            if (_clips.Count == 0) return;
            _segment = _clips.Dequeue();
            _segment.SetState(State.Playing, Context);
        }

        protected override bool OnTick(float elapsed, float deltaTime)
        {
            if (_segment != null)
            {
                if(_segment.Update(Context, elapsed, deltaTime))
                {
                    _finished.Enqueue(_segment);
                    _segment = null;

                    if (_clips.Count > 0)
                    {
                        _segment = _clips.Dequeue();
                        _segment.SetState(State.Playing, Context);
                    }
                }
            }
            
            return _segment == null;
        }

        protected override void OnFinish()
        {
            if (_segment != null)
            {
                _segment.SetState(State.Done, Context);
                _finished.Enqueue(_segment);
                _segment = null;
            }
            
            while (_clips.Count > 0)
            {
                var clip = _clips.Dequeue();
                clip.SetState(State.Done, Context);
                _finished.Enqueue(clip);
            }
            
            (_finished, _clips) = (_clips, _finished);
        }
        
        protected override void OnReplay()
        {
            if (_segment == null) return;
            
            _segment.clip.Replay(Context);
        }

        protected override void OnResetData()
        {
            _finished.Clear();
            _segment = null;
            _duration = 0f;
        }

        public void Clear()
        {
            _clips.Clear();
            _finished.Clear();
            _segment = null;
            _duration = 0f;
        }
    }

    public sealed class TaskParallel : TaskTrack
    {
        private float _duration;

        private bool _started;

        private readonly List<ClipWrapper> _clips = new List<ClipWrapper>();
        
        public override bool CanInterrupted() 
        {
            bool canInterrupted = true;
            
            for (int i = _clips.Count - 1; i >= 0; i--)
            {
                ClipWrapper clipWrapper = _clips[i];
                if (clipWrapper.state == State.Playing)
                {
                    canInterrupted = clipWrapper.clip.CanInterrupted();
                    if (!canInterrupted)
                        break;
                }
            }

            return canInterrupted;
        }

        public void Insert(TaskNode clip, float time = 0f)
        {
            if (clip == null) return;

            if (_started)
                throw new Exception("Can't insert playable when track is playing");
            
            ClipWrapper clipWrapper = new ClipWrapper()
            {
                startTime = time,
                endTime = time + clip.duration,
                clip = clip,
                state = State.NotStarted
            };
            
            _duration = Mathf.Max(_duration, clipWrapper.endTime);
            _clips.Add(clipWrapper);
        }

        public void Remove(TaskNode clip)
        {
            if (_started)
                throw new Exception("Can't remove playable when track is playing");

            _duration = 0f;

            for (int i = _clips.Count - 1; i >= 0; i--)
            {
                ClipWrapper clipWrapper = _clips[i];
                if (clipWrapper.clip == clip)
                {
                    _clips.RemoveAt(i);
                    continue;
                }

                _duration = Mathf.Max(_duration, clipWrapper.endTime);
            }
        }

        public void Clear()
        {
            _clips.Clear();
            _duration = 0f;
        }

        public override float duration => _duration;

        protected override void OnStart()
        {
            _started = true;

            for (int i = _clips.Count - 1; i >= 0; i--)
            {
                ClipWrapper clipWrapper = _clips[i];
                clipWrapper.state = State.NotStarted;
            }
        }

        protected override bool OnTick(float elapsed, float deltaTime)
        {
            bool hasRun = false;

            for (int i = _clips.Count - 1; i >= 0; i--)
            {
                ClipWrapper clip = _clips[i];
                if (!clip.Update(Context, elapsed, deltaTime))
                    hasRun = true;
            }
            
            return !hasRun;
        }

        protected override void OnFinish()
        {
            _started = false;
            foreach (var clipWrapper in _clips)
            {
                if (clipWrapper.state != State.Done)
                    clipWrapper.SetState(State.Done, Context);
            }
        }
        
        protected override void OnReplay()
        {
            foreach (var clipWrapper in _clips)
            {
                if (clipWrapper.state == State.Playing)
                    clipWrapper.clip.Replay(Context);
            }
        }

        protected override void OnResetData()
        {
            _started = false;
        }
    }
}