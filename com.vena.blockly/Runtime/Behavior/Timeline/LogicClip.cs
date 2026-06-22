// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{
    [BlocklySource("时间线/剪辑", typeof(LogicClip))]
    public sealed class LogicClip : ClipSource, ITimelineClip
    {
        [ExpressionSignature]
        [BlocklySourceSlot("开始时", 1)]
        public LogicGraph onBegin;

        [ExpressionSignature(typeof(void), typeof(FrameInfo))]
        [BlocklySourceSlot("每帧", 2)]
        public LogicGraph onFrame;

        [ExpressionSignature(typeof(void), typeof(FrameInfo))]
        [BlocklySourceSlot("结束时", 3)]
        public LogicGraph onEnd;

        private Timeline _timeline;
        private LogicClip _source;
        private LogicGraph.Blockly _onBegin;
        private LogicGraph.Blockly _onFrame;
        private LogicGraph.Blockly _onEnd;

        Timeline ITimelineObject.timeline => _timeline;

        void ITimelineClip.OnCreate(Timeline timeline, IBlocklySource source)
        {
            _timeline = timeline;
            _source = (LogicClip)source;
            _onBegin = _timeline.blockly.CreateBlockly(_source.onBegin);
            _onFrame = _timeline.blockly.CreateBlockly(_source.onFrame);
            _onEnd = _timeline.blockly.CreateBlockly(_source.onEnd);
        }

        void ITimelineClip.Begin() { _onBegin?.Invoke(); }
        void ITimelineClip.OnFrame(in FrameInfo frameInfo) { _onFrame?.Invoke(frameInfo); }
        void ITimelineClip.End(in FrameInfo frameInfo) { _onEnd?.Invoke(frameInfo); }

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
