// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly.Tests.BehaviorRuntime
{

    #region 测试行为1

    public class SampleBehaviorImpl1 : IBehavior
    {
        public void Start(BehaviorGraph.Blockly graph)
        {
        }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            return BehaviorResult.Done;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
        }
    }

    /// <summary>手写 BehaviorSource 参考样例（无槽位）。</summary>
    [BlocklySource("测试行为/测试行为1", typeof(SampleBehaviorImpl1Source.Node))]
    public sealed class SampleBehaviorImpl1Source : BehaviorNodeSource<SampleBehaviorImpl1>
    {
        sealed class Node : BehaviorNode<SampleBehaviorImpl1Source, SampleBehaviorImpl1>
        {
            protected override void Initialize()
            {
            }

            protected override void InitializeProperties(SampleBehaviorImpl1 behaviorImpl)
            {
            }

            protected override void CleanProperties(SampleBehaviorImpl1 behaviorImpl)
            {
            }

            protected override void OnBeforeDestroy()
            {

            }
        }
    }

    #endregion

    #region 测试行为2

    public class SampleBehaviorImpl2 : IBehavior
    {
        public string message;

        public void Start(BehaviorGraph.Blockly graph)
        {
        }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            return BehaviorResult.Done;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
        }
    }

    /// <summary>手写 BehaviorSource 参考样例（含一个 LogicGraph 字符串槽）。</summary>
    [BlocklySource("测试行为/测试行为2", typeof(SampleBehaviorImpl2Source.Node))]
    public sealed class SampleBehaviorImpl2Source : BehaviorNodeSource<SampleBehaviorImpl2>
    {
        [ExpressionSignature(typeof(string))]
        [BlocklySourceSlot("消息", 1)]
        public LogicGraph message;

        sealed class Node : BehaviorNode<SampleBehaviorImpl2Source, SampleBehaviorImpl2>
        {
            private LogicGraph.Blockly _message;

            protected override void Initialize()
            {
                _message = blockly.CreateBlockly(source.message);
            }

            protected override void InitializeProperties(SampleBehaviorImpl2 behaviorImpl)
            {
                behaviorImpl.message = _message.Call<string>();
            }

            protected override void CleanProperties(SampleBehaviorImpl2 behaviorImpl)
            {
                behaviorImpl.message = null;
            }

            protected override void OnBeforeDestroy()
            {
                blockly.DestroyBlockly(_message);
                _message = null;
            }
        }
    }

    #endregion
}
