// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    /// <summary>
    /// Timeline 唯一 Clip 类型：嵌入三条 LogicGraph（onBegin / onFrame / onEnd），
    /// 同时既是序列化数据持有者（UClipSource）也是运行期实现（ITimelineClip）。
    /// </summary>
    [BlocklySource("时间线/剪辑", typeof(UClip))]
    public sealed class UClip : UClipSource, ITimelineClip
    {
        [ExpressionSignature]
        [BlocklySourceSlot("开始时", 1)]
        public LogicGraph onBegin;

        [ExpressionSignature(typeof(void), typeof(UFrameInfo))]
        [BlocklySourceSlot("每帧", 2)]
        public LogicGraph onFrame;

        [ExpressionSignature(typeof(void), typeof(UFrameInfo))]
        [BlocklySourceSlot("结束时", 3)]
        public LogicGraph onEnd;

        private Timeline _timeline;

        private UClip _source;

        private LogicGraph.Blockly _onBegin;
        private LogicGraph.Blockly _onFrame;
        private LogicGraph.Blockly _onEnd;

        Timeline ITimelineObject.timeline => _timeline;

        void ITimelineClip.OnCreate(Timeline timeline, IBlocklySource source)
        {
            _timeline = timeline;
            _source = (UClip)source;

            _onBegin = _timeline.blockly.CreateBlockly(_source.onBegin);
            _onFrame = _timeline.blockly.CreateBlockly(_source.onFrame);
            _onEnd = _timeline.blockly.CreateBlockly(_source.onEnd);
        }

        void ITimelineClip.Begin()
        {
            _onBegin?.Invoke();
        }

        void ITimelineClip.OnFrame(in UFrameInfo frameInfo)
        {
            _onFrame?.Invoke(frameInfo);
        }

        void ITimelineClip.End(in UFrameInfo frameInfo)
        {
            _onEnd?.Invoke(frameInfo);
        }

        void ITimelineObject.OnDestroy()
        {
            _timeline.blockly.DestroyBlockly(_onBegin);
            _timeline.blockly.DestroyBlockly(_onFrame);
            _timeline.blockly.DestroyBlockly(_onEnd);

            _onBegin = null;
            _onFrame = null;
            _onEnd = null;
        }
    }

}
