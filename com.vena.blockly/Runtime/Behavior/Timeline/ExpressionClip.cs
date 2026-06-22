// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    /// <summary>
    /// Expression 驱动的时间轴剪辑源数据。
    /// </summary>
    [BlocklySource("时间线/表达式剪辑", typeof(ExpressionClip.Object))]
    public sealed class ExpressionClip : UClipSource
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

        /// <summary>
        /// Expression 驱动的时间轴剪辑运行时节点。
        /// 通过 UGCWorld 工厂创建 LogicGraph 作为局部作用域。
        /// </summary>
        sealed class Object : UClip<ExpressionClip>
        {
            private LogicGraph.Blockly _onBegin;
            private LogicGraph.Blockly _onFrame;
            private LogicGraph.Blockly _onEnd;

            protected override void OnCreate()
            {
                _onBegin = timeline.blockly.CreateBlockly(source.onBegin);
                _onFrame = timeline.blockly.CreateBlockly(source.onFrame);
                _onEnd = timeline.blockly.CreateBlockly(source.onEnd);
            }

            public override void Begin()
            {
                _onBegin?.Invoke();
            }

            public override void OnFrame(in UFrameInfo frameInfo)
            {
                _onFrame?.Invoke(frameInfo);
            }

            public override void End(in UFrameInfo frameInfo)
            {
                _onEnd?.Invoke(frameInfo);
            }

            protected override void OnDestroy()
            {
                timeline.blockly.DestroyBlockly(_onBegin);
                timeline.blockly.DestroyBlockly(_onFrame);
                timeline.blockly.DestroyBlockly(_onEnd);

                _onBegin = null;
                _onFrame = null;
                _onEnd = null;
            }
        }
    }


}
