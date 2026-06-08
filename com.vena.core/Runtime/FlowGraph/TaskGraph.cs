using System;
using System.Collections.Generic;
using Vena;

namespace Vena
{
    /// <summary>
    /// 行为图 支持行为组合+复用
    /// </summary>
    public class TaskGraph
    {
        public event Action<ITaskContext> onStartEvent, onReplayEvent, onEndEvent;
        private readonly TaskParallel _parallelTrack = new TaskParallel();
        private readonly TaskSequence _sequenceTrack = new TaskSequence();
        private ITaskContext _context;
        private TaskNode _playableNode;
        private bool _isPlaying;
        private int _occupied;

        public bool isPlaying => _isPlaying;

        public Type actionType => _context?.GetType() ?? null;

        public ITaskContext Context => _context;

        public virtual bool CanInterruptBy(ITaskContext context)
        {
            return _isPlaying && _sequenceTrack.CanInterrupted();
        }

        public virtual bool CanInterruptByInput()
        {
            return _isPlaying;
        }

        private TaskNode NewAction(Type actionClassType)
        {
            return Activator.CreateInstance(actionClassType) as TaskNode;
        }

        private void DeleteAction(TaskNode actionNode)
        {
        }

        public void BeginGraph(ITaskContext newParam)
        {
            if (_occupied > 0)
                throw new Exception(
                    $"PlayableGraph is occupied[{_occupied}], can't begin new graph - {newParam.GetType().Name}!");

            if (_isPlaying)
                throw new Exception($"PlayableGraph is playing, can't begin new graph - {newParam.GetType().Name}!");

            _isPlaying = true;
            _occupied = 1;

            DeleteAction(_playableNode);
            _playableNode = null;

            _context?.Dispose();
            _context = newParam;

            // create action ...
            Type actionClassType = _context.GetExecuteClassType();
            _playableNode = NewAction(actionClassType);
            if (_playableNode == null)
                throw new Exception($"PlayableGraph can't create action - {actionClassType.Name}!");

            // register event ...
            if (_context is IHasLogicEventClip keyEventAction)
            {
                TaskEvent[] eventClips = keyEventAction.GetEvents();
                if (eventClips != null && eventClips.Length > 0 )
                {
                    foreach (var eventClip in eventClips)
                        _playableNode.AddEvent(eventClip);
                }
            }

            // prepare graph ...
            OnBeforePrepareGraph();

            // before action ...
            List<TaskNode> tempList = new List<TaskNode>();
            int length = GetBeforeActions(tempList);
            for (int i = 0; i < length; i++)
                _sequenceTrack.Enqueue(tempList[i]);

            // doing action ...
            tempList.Clear();
            length = GetParallelActions(tempList);
            for (int i = 0; i < length; i++)
                _parallelTrack.Insert(tempList[i]);

            _parallelTrack.Insert(_playableNode);
            _sequenceTrack.Enqueue(_parallelTrack);

            // after action ...
            tempList.Clear();
            length = GetAfterActions(tempList);
            for (int i = 0; i < length; i++)
                _sequenceTrack.Enqueue(tempList[i]);

            tempList.Clear();
            OnAfterPrepareGraph();

            // do begin ...
            onStartEvent?.Invoke(_context);
            _context.OnBeforeStart();
            _sequenceTrack.Start(_context);
            _occupied = 0;
        }
        
        public bool Tick(float deltaTime)
        {
            if (!_isPlaying) return true;
            return _sequenceTrack.Tick(deltaTime);
        }

        public void ReplayGraph(ITaskContext newParam)
        {
            if (_isPlaying)
            {
                if (newParam != _context)
                {
                    _context.CopyFrom(newParam);
                    _sequenceTrack.Replay(_context);
                    onReplayEvent?.Invoke(_context);
                    newParam?.Dispose();
                }
                else
                {
                    _sequenceTrack.Replay(_context);
                    onReplayEvent?.Invoke(_context);
                }

                return;
            }

            throw new Exception($"PlayableGraph is not playing, can't replay graph - {newParam.GetType().Name}!");
        }

        public void EndGraph()
        {
            if (_occupied > 0)
                throw new Exception($"PlayableGraph is occupied[{_occupied}], can't end graph!");

            if (_isPlaying)
            {
                _occupied = 2;
                _sequenceTrack.Finish();

                onEndEvent?.Invoke(_context);
                DeleteAction(_playableNode);
                _playableNode = null;

                _parallelTrack.Clear();
                _sequenceTrack.Clear();

                if (null != _context)
                {
                    _context.OnAfterFinish();
                    _context.Dispose();
                    _context = null;
                }

                _occupied = 0;
                _isPlaying = false;
            }
        }

        public void Clear()
        {
            EndGraph();
        }

        protected virtual void OnBeforePrepareGraph()
        {
        }

        protected virtual int GetBeforeActions(List<TaskNode> outList)
        {
            return 0;
        }

        protected virtual int GetAfterActions(List<TaskNode> outList)
        {
            return 0;
        }
        
        protected virtual int GetParallelActions(List<TaskNode> outList)
        {
            return 0;
        }

        protected virtual void OnAfterPrepareGraph()
        {
        }
    }
}