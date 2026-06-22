// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Vena.Blockly
{


    public sealed class Timeline : CompositeBehavior<TimelineSource>
    {
        public readonly Group group;

        private float _interval = 1f / 30;

        private int _frameRate = 30;

        private float _previousTime;

        private int _currentFrame;

        private bool _timelineRunning;

        public int frameRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _frameRate;
        }

        public Timeline()
        {
            group = new Group(this);
        }

        public override string ToString()
        {
            return $"UGraphNode_Timeline:(frameRate = {_frameRate}, frameCount = {group.TotalFrame})";
        }

        protected override void Initialize()
        {
            _frameRate = source.frameRate;

            _interval = 1f / frameRate;

            ((ITrack)group).SetSource(source.group);
        }

        private void StopUnsafe()
        {
            _timelineRunning = false;

            _currentFrame = 0;

            if (group is ITrack track1)
            {
                track1.Reset();
            }
        }

        #region Behavior

        protected override void OnResetData()
        {
        }

        protected override void OnStart()
        {
            _currentFrame = 0;

            _previousTime = 0;

            _timelineRunning = true;
        }

        protected override BehaviorResult OnTick(float deltaTime)
        {
            using var capture = new ExceptionCapture(this);

            int iFrame = (int)System.Math.Floor(elapsedTime / _interval);

            while (_currentFrame < iFrame)
            {
                _currentFrame++;

                float realDeltaTime = elapsedTime - _previousTime;

                _previousTime = elapsedTime;

                group.OnFrame(_currentFrame, realDeltaTime);
            }

            if (_currentFrame >= group.TotalFrame)
            {
                StopUnsafe();
            }

            return _timelineRunning ? BehaviorResult.Running : BehaviorResult.Done;
        }

        protected override void OnLateTick(float deltaTime)
        {
        }

        protected override void OnFinish()
        {
            if (_timelineRunning)
            {
                using var capture = new ExceptionCapture(this);

                StopUnsafe();
            }
        }

        protected override void OnDestroy()
        {
             // destroy all tracks
             group.Cleanup();
        }

        #endregion

        #region ExceptionCapture

        readonly struct ExceptionCapture : IDisposable
        {
            private readonly Timeline _timeline;

            public ExceptionCapture(Timeline timeline)
            {
                _timeline = timeline;

                if (_timeline._capturing)
                {
                    throw new InvalidOperationException("Cannot start a ExceptionCapture while it is capturing.");
                }

                _timeline._capturing = true;
            }

            public void Dispose()
            {
                _timeline._capturing = false;

                var exception = _timeline._raisedException;

                _timeline._raisedException = null;

                exception?.Throw();
            }
        }

        private ExceptionDispatchInfo _raisedException;

        private bool _capturing;

        private void RaiseException(Exception e)
        {
            if (_raisedException == null)
            {
                _raisedException = ExceptionDispatchInfo.Capture(e);
            }
        }

        #endregion


        /// <summary>
        /// 轨道基类
        /// </summary>
        public interface ITrack
        {
            int TotalFrame { get; }

            void OnFrame(int frame, float deltaTime);

            void Reset();

            void SetSource(TimelineSource.ISource source);

            void Cleanup();
        }

        #region Clip Track

        /// <summary>
        /// 串行轨道
        /// </summary>
        public class Track<TClip, TInput> : ITrack
            where TClip : ITimelineClip
            where TInput : ClipSource, new()
        {
            #region Clip

            class Clip
            {
                public readonly short StartFrame;

                public readonly short TotalFrame;

                public readonly TInput source;

                private readonly TClip _output;

                private short _currentFrame;

                private float _elapsedTime;

                public Clip(TInput source, TClip clip, short startFrame, short totalFrame)
                {
                    StartFrame = startFrame;

                    TotalFrame = totalFrame;

                    this.source = source;

                    _output = clip;

                    _elapsedTime = _currentFrame = 0;
                }

                public bool OnFrame(Timeline timeline, float deltaTime)
                {
                    try
                    {
                        FrameInfo frameInfo;

                        // do begin
                        if (_currentFrame == 0)
                        {
                            frameInfo = new FrameInfo(_currentFrame, TotalFrame, 0, 0);

                            _output.Begin();

                            _output.OnFrame(frameInfo);
                        }
                        // do frame
                        else
                        {
                            _elapsedTime += deltaTime;

                            frameInfo = new FrameInfo(_currentFrame, TotalFrame, _elapsedTime, deltaTime);

                            _output.OnFrame(frameInfo);
                        }

                        // check end
                        if (++_currentFrame >= TotalFrame)
                        {
                            _output.End(frameInfo);

                            return true;
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        timeline.RaiseException(e);

                        return true;
                    }
                }

                public void Reset(Timeline timeline)
                {
                    // make sure call Begin
                    if (_currentFrame <= 0)
                    {
                        OnFrame(timeline, 0);
                    }

                    if (_currentFrame > 0)
                    {
                        int frame = _currentFrame;

                        float elapsed = _elapsedTime;

                        _elapsedTime = 0;

                        _currentFrame = 0;

                        if (frame < TotalFrame)
                        {
                            try
                            {
                                var frameInfo = new FrameInfo(frame, TotalFrame, elapsed, 0);

                                _output.End(frameInfo);
                            }
                            catch (Exception e)
                            {
                                timeline.RaiseException(e);
                            }
                        }
                    }
                }

                public void Destroy()
                {
                    ((ITimelineObject)_output).OnDestroy();
                }
            }

            #endregion

            protected readonly Group _group;

            private short _totalFrame, _currentFrame;

            private readonly Dictionary<short, Clip> _clips;

            private readonly Dictionary<short, ISignalObject> _signals;

            private readonly List<Clip> _playingClips;

            public Track(Group group)
            {
                _group = group;

                _clips = new Dictionary<short, Clip>();

                _signals = new Dictionary<short, ISignalObject>();

                _playingClips = new List<Clip>();

                _currentFrame = 0;
            }

            public int TotalFrame
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _totalFrame;
            }

            public void OnFrame(int frame, float deltaTime)
            {
                // check frame
                if (_currentFrame >= _totalFrame)
                {
                    return;
                }

                _currentFrame++;

                Timeline timeline = _group.Timeline;

                if (_currentFrame != frame)
                {
                    _currentFrame = _totalFrame;

                    timeline.RaiseException(
                        new Exception($"Frame error, currentFrame = {_currentFrame}, frame = {frame}"));

                    return;
                }

                // add playing segments
                if (_clips.TryGetValue(_currentFrame, out var clip))
                {
                    _playingClips.Add(clip);
                }

                // execute signals
                if (_signals.TryGetValue(_currentFrame, out var signal))
                {
                    try
                    {
                        signal.Execute();
                    }
                    catch (Exception e)
                    {
                        timeline.RaiseException(e);
                    }
                }

                // update
                for (int i = _playingClips.Count - 1; i >= 0; i--)
                {
                    if (_playingClips[i].OnFrame(timeline, deltaTime))
                    {
                        _playingClips.RemoveAt(i);
                    }
                }
            }

            public void Reset()
            {
                Timeline timeline = _group.Timeline;

                foreach (var keyValue in _clips)
                {
                    keyValue.Value.Reset(timeline);
                }

                _playingClips.Clear();

                _currentFrame = 0;
            }

            public void RecalculateTotalFrame()
            {
                _totalFrame = 0;

                foreach (var clip in _clips.Values)
                {
                    int endFrame = clip.StartFrame + clip.TotalFrame;

                    _totalFrame = System.Math.Max(_totalFrame, (short)endFrame);
                }

                foreach (var signal in _signals.Keys)
                {
                    _totalFrame = System.Math.Max(_totalFrame, signal);
                }

                _group.RecalculateTotalFrame();
            }

            void ITrack.SetSource(TimelineSource.ISource source)
            {
                // Clear existing clips and signals
                _clips.Clear();
                _playingClips.Clear();
                _currentFrame = 0;

                if (!(source is TimelineSource.TrackSource<TClip, TInput> trackSource))
                {
                    throw new ArgumentException("Source is not a TrackSource");
                }

                // Instantiate new clips and signals from source
                if (trackSource.clips != null && trackSource.clips.Count > 0)
                {
                    foreach (var clip in trackSource.clips)
                    {
                        const bool recalculate = false;

                        AddClip(clip, recalculate);
                    }
                }

                if (trackSource.signals != null && trackSource.signals.Count > 0)
                {
                    foreach (var keyValue in trackSource.signals)
                    {
                        const bool recalculate = false;

                        AddSignal(keyValue.Key, keyValue.Value, recalculate);
                    }
                }

                RecalculateTotalFrame();
            }

            void ITrack.Cleanup()
            {
                _playingClips.Clear();

                _currentFrame = 0;

                foreach (var clip in _clips.Values)
                {
                    clip.Destroy();
                }
                _clips.Clear();

                foreach (var signalObject in _signals.Values)
                {
                    signalObject.OnDestroy();
                }
                _signals.Clear();
            }

            private void AddClip(TimelineSource.TrackSource<TClip, TInput>.Clip clip, bool recalculateTotalFrame)
            {
                int startFrame = System.Math.Max(1, clip.frame);

                if (_clips.ContainsKey((short)startFrame))
                {
                    throw new ArgumentException("Clip already exists, startFrame = " + startFrame);
                }

                TInput clipSource = (TInput)clip.source;

                var frameCount = (short)System.Math.Ceiling(clipSource.Duration * _group.Timeline.frameRate);

                frameCount = System.Math.Max(frameCount, (short)1);

                var timeline = _group.Timeline;

                Type clipType = BlocklySourceAttribute.GetNodeType(clipSource.GetType());

                var clipObject = (ITimelineClip)Activator.CreateInstance(clipType);

                clipObject.OnCreate(timeline, clipSource);

                Clip clip1 = new Clip(clipSource, (TClip)clipObject, (short)startFrame, frameCount);

                _clips.Add(clip1.StartFrame, clip1);

                if (recalculateTotalFrame)
                {
                    RecalculateTotalFrame();
                }
            }

            private void AddSignal(int frame, Signal signalSource, bool recalculateTotalFrame)
            {
                short frameCount = (short)System.Math.Max(1, frame);

                if (_signals.ContainsKey(frameCount))
                {
                    throw new ArgumentException("Signal already exists at frame " + frameCount);
                }

                var timeline = _group.Timeline;

                var signal = signalSource.CreateSignalObject(timeline);

                _signals.Add(frameCount, signal);

                if (recalculateTotalFrame)
                {
                    RecalculateTotalFrame();
                }
            }
        }

        #endregion

        #region Track Group

        /// <summary>
        /// 轨道组合
        /// </summary>
        public sealed class Group : ITrack
        {
            public readonly Timeline Timeline;

            private int _totalFrame;

            private readonly List<ITrack> _tracks;

            public Group(Timeline timeline)
            {
                Timeline = timeline;

                _totalFrame = 0;

                _tracks = new List<ITrack>();
            }

            public IReadOnlyCollection<ITrack> Tracks
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _tracks;
            }

            public int TotalFrame
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _totalFrame;
            }

            public void OnFrame(int frame, float deltaTime)
            {
                foreach (ITrack tack in _tracks)
                {
                    tack.OnFrame(frame, deltaTime);
                }
            }

            public void Reset()
            {
                foreach (ITrack tack in _tracks)
                {
                    tack.Reset();
                }
            }

            void ITrack.SetSource(TimelineSource.ISource source)
            {
                // Clear existing tracks
                _tracks.Clear();
                _totalFrame = 0;

                if (!(source is TimelineSource.GroupSource groupSource))
                {
                    throw new ArgumentException("Source is not a GroupSource");
                }

                // Instantiate new tracks from source
                if (groupSource.TrackList != null && groupSource.TrackList.Count > 0)
                {
                    foreach (TimelineSource.ISource src in groupSource.TrackList)
                    {
                        if (src is TimelineSource.GroupSource)
                        {
                            var newGroup = new Group(Timeline);

                            ((ITrack)newGroup).SetSource(source);

                            _tracks.Add(newGroup);

                            _totalFrame = System.Math.Max(_totalFrame, newGroup.TotalFrame);
                        }
                        else if (src is TimelineSource.ITrackSource trackSource)
                        {
                            var newTrack = trackSource.InstantiateTrack(this);

                            newTrack.SetSource(source);

                            _tracks.Add(newTrack);

                            _totalFrame = System.Math.Max(_totalFrame, newTrack.TotalFrame);
                        }
                    }
                }

                RecalculateTotalFrame();
            }

            public void RecalculateTotalFrame()
            {
                _totalFrame = 0;

                if (_tracks != null && _tracks.Count > 0)
                {
                    foreach (var sourceTrack in _tracks)
                    {
                        _totalFrame = System.Math.Max(_totalFrame, sourceTrack.TotalFrame);
                    }
                }
            }

            public void Cleanup()
            {
                foreach (var track in _tracks)
                {
                    track.Cleanup();
                }
                _tracks.Clear();
            }
        }

        #endregion
    }

    public interface ITimelineObject
    {
        Timeline timeline { get; }

        void OnDestroy();
    }
}
