// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    public readonly struct FrameInfo
    {
        /// <summary>
        /// 当前帧
        /// </summary>
        public readonly int frame;

        /// <summary>
        /// 总帧数
        /// </summary>
        public readonly int totalFrame;

        /// <summary>
        /// 百分比
        /// </summary>
        public readonly float percent;

        /// <summary>
        /// 已经过时间
        /// </summary>
        public readonly float elapsed;

        /// <summary>
        /// 间隔时间
        /// </summary>
        public readonly float deltaTime;

        public FrameInfo(int frame, int totalFrame, float elapsed, float deltaTime)
        {
            this.frame = frame;
            this.totalFrame = totalFrame;
            percent = frame * 1f / totalFrame;
            this.elapsed = elapsed;
            this.deltaTime = deltaTime;
        }

        public FrameInfo WithElapsed(float elapsed)
        {
            return new FrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public FrameInfo WithDeltaTime(float deltaTime)
        {
            return new FrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public FrameInfo WithFrame(int frame)
        {
            return new FrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public FrameInfo WithTotalFrame(int totalFrame)
        {
            return new FrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public override string ToString()
        {
            return $"FrameInfo: {frame}/{totalFrame}, {percent:P2}, {elapsed:F4}/{deltaTime:F4}";
        }
    }


    public interface ITimelineClip : ITimelineObject
    {
        void OnCreate(Timeline timeline, IBlocklySource source);

        void Begin();

        void OnFrame(in FrameInfo frameInfo);

        public void End(in FrameInfo frameInfo);
    }

    /// <summary>
    /// 剪辑源
    /// </summary>
    public abstract class ClipSource : IBlocklySource, IBlocklySerializable
    {
        public ulong InstanceId { get; set; } = 0;

        public float duration;

        public float Duration => duration;

        public virtual void Serialize(IBlocklySerializer writer) { }

        public virtual void Deserialize(IBlocklySerializer reader) { }
    }

    public interface IClip
    {
        void Begin();
        void OnFrame(in FrameInfo frameInfo);
        void End(in FrameInfo frameInfo);
    }

    public abstract class ClipSource<TImpl> : ClipSource where TImpl : class, IClip, new()
    {
    }

    public abstract class Clip<TSource> : ITimelineClip where TSource : ClipSource
    {
        public Timeline timeline { get; private set; }
        public TSource source { get; private set; }
        public float elapsedTime { get; private set; }
        public int frame { get; private set; }
        public bool isPlaying { get; private set; }

        Timeline ITimelineObject.timeline => timeline;

        void ITimelineClip.OnCreate(Timeline timeline, IBlocklySource source)
        {
            if (!(source is TSource tSource))
            {
                throw new System.ArgumentException(
                    $"{GetType().Name}.OnCreate({source}) error, source must be of type {typeof(TSource).Name}!");
            }
            this.timeline = timeline;
            this.source = tSource;
            Initialize();
        }

        void ITimelineClip.Begin()
        {
            if (isPlaying)
                throw new System.Exception($"{GetType().Name}.Begin() error, is already running!");
            elapsedTime = 0f;
            frame = 0;
            isPlaying = true;
            OnBegin();
        }

        void ITimelineClip.OnFrame(in FrameInfo frameInfo)
        {
            if (!isPlaying) return;
            elapsedTime = frameInfo.elapsed;
            frame = frameInfo.frame;
            OnFrame(frameInfo);
        }

        void ITimelineClip.End(in FrameInfo frameInfo)
        {
            if (!isPlaying) return;
            try { OnEnd(frameInfo); }
            finally { isPlaying = false; elapsedTime = 0f; frame = 0; }
        }

        void ITimelineObject.OnDestroy()
        {
            OnBeforeDestroy();
            timeline = null;
            source = null;
        }

        protected abstract void Initialize();
        protected abstract void OnBegin();
        protected abstract void OnFrame(in FrameInfo frameInfo);
        protected abstract void OnEnd(in FrameInfo frameInfo);
        protected abstract void OnBeforeDestroy();
    }

    public abstract class Clip<TSource, TImpl> : Clip<TSource>
        where TSource : ClipSource<TImpl>
        where TImpl : class, IClip, new()
    {
        private readonly TImpl _impl = new TImpl();

        protected sealed override void OnBegin()
        {
            InitializeProperties(_impl);
            _impl.Begin();
        }

        protected sealed override void OnFrame(in FrameInfo frameInfo)
        {
            _impl.OnFrame(frameInfo);
        }

        protected sealed override void OnEnd(in FrameInfo frameInfo)
        {
            try { _impl.End(frameInfo); }
            finally { CleanProperties(_impl); }
        }

        protected abstract void InitializeProperties(TImpl clipImpl);
        protected abstract void CleanProperties(TImpl clipImpl);
    }

}
