// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Blockly
{

    internal interface ISignalObject : ITimelineObject
    {
        void Execute();
    }

    /// <summary>
    /// Expression 驱动的信号源数据（可视化配置路径）
    /// </summary>
    [BlocklySource("时间线/信号", typeof(Signal.Object))]
    public sealed class Signal : IBlocklySource, IBlocklySerializable
    {
        public ulong InstanceId { get; set; } = 0;

        [ExpressionSignature]
        [BlocklySourceSlot("表达式", 1)]
        public ExpressionBlockly source;

        public void Serialize(IBlocklySerializer writer) { }

        public void Deserialize(IBlocklySerializer reader) { }

        internal ISignalObject CreateSignalObject(Timeline timeline)
        {
            return new Object(timeline, this);
        }

        sealed class Object : ISignalObject
        {
            private ExpressionBlockly.Blockly _blockly;

            private Signal _signal;

            public Timeline timeline { get; private set; }

            public Object(Timeline timeline, Signal source)
            {
                this.timeline = timeline;

                _signal = source;

                _blockly = timeline.blockly.CreateBlockly(_signal.source);
            }

            void ITimelineObject.OnDestroy()
            {
                timeline.blockly.DestroyBlockly(_blockly);

                _blockly = null;
            }

            void ISignalObject.Execute()
            {
                _blockly.Invoke();
            }
        }
    }
}
