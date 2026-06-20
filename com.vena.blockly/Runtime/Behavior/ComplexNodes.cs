using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena.Blockly
{

    /// <summary>
    /// UFlowComplexNode is an abstract class that represents a complex node in a graph.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public abstract class UBehaviorComplexNode<TSource> : CompositeBehavior<TSource>
        where TSource : BehaviorNodeSource
    {
        // 0: not started, 1: playing, 2: done
        protected enum State
        {
            NotStarted,
            Playing,
            Done
        }

        protected class NodeWrapper
        {
            public readonly UBehaviorComplexNode<TSource> parent;
            public readonly IBehaviorNode clip;
            public State state;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator bool(NodeWrapper wrapper)
            {
                return wrapper != null && wrapper.clip != null && wrapper.parent != null;
            }

            public NodeWrapper(UBehaviorComplexNode<TSource> parent, IBehaviorNode clip)
            {
                this.parent = parent;
                this.clip = clip;
                state = State.NotStarted;
            }

            public void SetState(State state1)
            {
                if (state != state1)
                {
                    state = state1;

                    if (state == State.Playing)
                    {
                        try
                        {
                            clip.Start();
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

            public bool Update(float deltaTime)
            {
                if (state == State.Playing)
                {
                    try
                    {
                        if (clip.Tick(deltaTime) != BehaviorResult.Running)
                            SetState(State.Done);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"{clip.GetType().Name}.Update() ! {e} !!");
                    }
                }

                return state == State.Done;
            }

            public void LateUpdate(float deltaTime)
            {
                if (state == State.Playing)
                {
                    try
                    {
                        clip?.LateTick(deltaTime);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"{clip.GetType().Name}.LateUpdate() ! {e} !!");
                    }
                }
            }
        }
    }

    [UgcSource("行为组合/并行节点", typeof(ParallelNode.Node))]
    public sealed class ParallelNode : BehaviorNodeSource
    {
        [UgcSourceProperty("子节点列表", 1)]
        public BehaviorNodeSource[] sources;

        sealed class Node : UBehaviorComplexNode<ParallelNode>
        {
            private readonly List<NodeWrapper> _clips = new List<NodeWrapper>();

            protected override void Initialize()
            {
                if (source.sources == null || source.sources.Length == 0)
                    throw new Exception("Parallel source is empty");

                foreach (var clipSource in source.sources)
                {
                    IBehaviorNode clip = blockly.CreateBlock(clipSource);
                    if (clip != null)
                    {
                        _clips.Add(new NodeWrapper(this, clip));
                        continue;
                    }

                    blockly.Host?.Logger?.Error("Parallel source contains null clip source");
                }
            }

            protected override void OnStart()
            {
                for (int i = _clips.Count - 1; i >= 0; i--)
                {
                    NodeWrapper wrapper = _clips[i];
                    wrapper.state = State.NotStarted;
                    wrapper.SetState(State.Playing);
                }
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                bool hasRun = false;

                for (int i = _clips.Count - 1; i >= 0; i--)
                {
                    NodeWrapper wrapper = _clips[i];
                    if (!wrapper.Update(deltaTime))
                        hasRun = true;
                }

                return hasRun ? BehaviorResult.Running : BehaviorResult.Done;
            }

            protected override void OnLateTick(float deltaTime)
            {
                for (int i = _clips.Count - 1; i >= 0; i--)
                {
                    NodeWrapper wrapper = _clips[i];
                    wrapper.LateUpdate(deltaTime);
                }
            }

            protected override void OnFinish()
            {
                foreach (var wrapper in _clips)
                {
                    wrapper.SetState(State.Done);
                }
            }

            protected override void OnResetData()
            {
            }

            protected override void OnDestroy()
            {
                foreach (var wrapper in _clips)
                {
                    blockly.DestroyBlock(wrapper.clip);
                }
                _clips.Clear();
            }
        }
    }

    [UgcSource("行为组合/序列节点", typeof(SequenceNode.Node))]
    public sealed class SequenceNode : BehaviorNodeSource
    {
        [UgcSourceProperty("子节点列表", 1)]
        public BehaviorNodeSource[] sources;

        sealed class Node : UBehaviorComplexNode<SequenceNode>
        {
            private Queue<NodeWrapper> _clips = new Queue<NodeWrapper>();
            private Queue<NodeWrapper> _finished = new Queue<NodeWrapper>();

            private bool _tickValid;
            private NodeWrapper _segment;

            protected override void Initialize()
            {
                if (source.sources == null || source.sources.Length == 0)
                    throw new Exception("Sequence source is empty");

                foreach (var clipSource in source.sources)
                {
                    IBehaviorNode clip = blockly.CreateBlock(clipSource);
                    if (clip != null)
                    {
                        _clips.Enqueue(new NodeWrapper(this, clip));
                        continue;
                    }
                    blockly.Host?.Logger?.Error("Sequence source contains null clip source");
                }
            }

            protected override void OnStart()
            {
                if (_segment != null)
                    throw new Exception("Can't start when Sequence is playing");

                if (_clips.Count == 0) return;
                _tickValid = false;
                _segment = _clips.Dequeue();
                _segment.SetState(State.Playing);
            }

            protected override BehaviorResult OnTick(float deltaTime)
            {
                if (_segment != null)
                {
                    // 标记为有效
                    _tickValid = true;
                    if (_segment.Update(deltaTime))
                    {
                        if (!_tickValid)
                            return BehaviorResult.Running;

                        _finished.Enqueue(_segment);
                        _segment = null;

                        if (_clips.Count > 0)
                        {
                            _segment = _clips.Dequeue();
                            _segment.SetState(State.Playing);
                        }
                    }
                }

                return null == _segment ? BehaviorResult.Done : BehaviorResult.Running;
            }

            protected override void OnLateTick(float deltaTime)
            {
                _segment?.LateUpdate(deltaTime);
            }

            protected override void OnFinish()
            {
                if (_segment != null)
                {
                    _segment.SetState(State.Done);
                    _finished.Enqueue(_segment);
                    _segment = default;
                }

                while (_clips.Count > 0)
                {
                    var clip = _clips.Dequeue();
                    clip.SetState(State.Done);
                    _finished.Enqueue(clip);
                }

                (_finished, _clips) = (_clips, _finished);
                _tickValid = false;
            }

            protected override void OnResetData()
            {
                _finished.Clear();
                _segment = default;
                _tickValid = false;
            }

            protected override void OnDestroy()
            {
                foreach (var wrapper in _clips)
                {
                    blockly.DestroyBlock(wrapper.clip);
                }
                _clips.Clear();
                _finished.Clear();
                _segment = default;
                _tickValid = false;
            }
        }
    }

}
