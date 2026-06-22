// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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


    public interface ITimelineClip : ITimelineObject
    {
        void OnCreate(Timeline timeline, IBlocklySource source);

        void Begin();

        void OnFrame(in UFrameInfo frameInfo);

        public void End(in UFrameInfo frameInfo);
    }

    /// <summary>
    /// 剪辑源
    /// </summary>
    public abstract class UClipSource : IBlocklySource, IBlocklySerializable
    {
        public ulong InstanceId { get; set; } = 0;

        public float duration;

        public float Duration => duration;

        public virtual void Serialize(IBlocklySerializer writer) { }

        public virtual void Deserialize(IBlocklySerializer reader) { }
    }

}
