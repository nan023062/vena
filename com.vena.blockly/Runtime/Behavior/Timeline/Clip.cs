// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    public readonly struct UFrameInfo
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

        public UFrameInfo(int frame, int totalFrame, float elapsed, float deltaTime)
        {
            this.frame = frame;
            this.totalFrame = totalFrame;
            percent = frame * 1f / totalFrame;
            this.elapsed = elapsed;
            this.deltaTime = deltaTime;
        }

        public UFrameInfo WithElapsed(float elapsed)
        {
            return new UFrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public UFrameInfo WithDeltaTime(float deltaTime)
        {
            return new UFrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public UFrameInfo WithFrame(int frame)
        {
            return new UFrameInfo(frame, totalFrame, elapsed, deltaTime);
        }

        public UFrameInfo WithTotalFrame(int totalFrame)
        {
            return new UFrameInfo(frame, totalFrame, elapsed, deltaTime);
        }
        
        public override string ToString()
        {
            return $"FrameInfo: {frame}/{totalFrame}, {percent:P2}, {elapsed:F4}/{deltaTime:F4}";
        }
    }


    internal interface ITimelineClip : ITimelineObject
    {
        void OnCreate(Timeline timeline, IBlocklySource source);

        void Begin();

        void OnFrame(in UFrameInfo frameInfo);

        public void End(in UFrameInfo frameInfo);
    }

    /// <summary>
    /// 时间线剪辑接口实现
    /// </summary>
    public interface IUClip
    {
        void Begin(Timeline timeline);
        
        void OnFrame(Timeline timeline, in UFrameInfo frameInfo);
        
        void End(Timeline timeline, in UFrameInfo frameInfo);
    }

    /// <summary>
    /// 剪辑源
    /// </summary>
    public abstract class UClipSource : IBlocklySource, IBlocklySerializable
    {
        public ulong Guid { get; set; } = 0;

        public float duration;

        public float Duration => duration;

        public virtual void Serialize(IBlocklySerializer writer) { }

        public virtual void Deserialize(IBlocklySerializer reader) { }
    }
        
    public abstract class UClip<TSource> : ITimelineClip where TSource : UClipSource
    {
        public Timeline timeline { get; private set; }
        
        public TSource source { get; private set;  }
        
        void ITimelineClip.OnCreate(Timeline timeline, IBlocklySource source)
        {
            if (!(source is TSource tSource))
            {
                throw new InvalidOperationException($"Source type mismatch. Expected {typeof(TSource)}, got {source.GetType()}.");
            }
            
            this.timeline = timeline;
            
            this.source = tSource;
            
            OnCreate();
        }
        
        void ITimelineObject.OnDestroy()
        {
            OnDestroy();
        }

        public abstract void Begin();

        public abstract void OnFrame(in UFrameInfo frameInfo);

        public abstract void End(in UFrameInfo frameInfo);
        
        protected abstract void OnCreate();
        
        protected abstract void OnDestroy();
    }

    /// <summary>
    /// 剪辑源
    /// </summary>
    public abstract class UClipSource<TImpl> : UClipSource where TImpl : class, IUClip, new()
    {

    }

    public abstract class UClip<TSource, TImpl> : UClip<TSource> 
        where TSource : UClipSource<TImpl>
        where TImpl : class, IUClip, new()
    {
        private TImpl _impl;
        
        protected sealed override void OnCreate()
        {
            _impl = new TImpl();
            
            Initialize();
        }
        
        public sealed override void Begin()
        {
            // Initialize the implementation
            InitializeProperties(_impl);
                
            // Call the implementation's Begin method
            _impl.Begin(timeline);
        }

        public sealed override void OnFrame(in UFrameInfo frameInfo)
        {
            _impl.OnFrame( timeline, in frameInfo);
        }

        public sealed override void End(in UFrameInfo frameInfo)
        {
            _impl.End( timeline, in frameInfo);

            CleanProperties(_impl);
        }

        protected sealed override void OnDestroy()
        {
            OnBeforeDestroy();
            
            _impl = null;
        }

        /// <summary>
        /// Set up the behavior when created.
        /// </summary>
        protected abstract void Initialize();
            
        /// <summary>
        /// Initialize properties of the behavior implementation.
        /// </summary>
        /// <param name="behaviorImpl"></param>
        protected abstract void InitializeProperties(TImpl behaviorImpl);

        /// <summary>
        /// Clean up properties of the behavior implementation.
        /// </summary>
        /// <param name="behaviorImpl"></param>
        protected abstract void CleanProperties(TImpl behaviorImpl);

        protected abstract void OnBeforeDestroy();
    }

}
