// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    /// <summary>
    /// 最小叶子 IBehaviorImpl：Start / Tick / Finish 各打一行带 greeting 的日志，
    /// Tick 当帧返回 Done（一帧式叶子），LateTick 留空。
    /// </summary>
    public sealed class HelloBehaviorImpl : IBehaviorImpl
    {
        public string greeting;

        public void Start(BehaviorGraph.Blockly graph)
        {
            Debug.Log($"[HelloBehavior] Start: {greeting}");
        }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            Debug.Log($"[HelloBehavior] Tick: {greeting}");
            return BehaviorResult.Done;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime)
        {
        }

        public void Finish(BehaviorGraph.Blockly graph)
        {
            Debug.Log($"[HelloBehavior] Finish: {greeting}");
        }
    }

    /// <summary>
    /// HelloBehaviorImpl 的 source + Node 装配。结构对照同目录 TestBehaviorImpl.cs（迁入 demo 后的 SampleBehaviorImpl1Source 形态）。
    /// </summary>
    [UgcSource("示例行为/Hello", typeof(HelloBehaviorSource.Node))]
    public sealed class HelloBehaviorSource : BehaviorNodeSource<HelloBehaviorImpl>
    {
        public string greeting;

        sealed class Node : BehaviorNode<HelloBehaviorSource, HelloBehaviorImpl>
        {
            protected override void Initialize() { }

            protected override void InitializeProperties(HelloBehaviorImpl impl)
            {
                impl.greeting = source.greeting;
            }

            protected override void CleanProperties(HelloBehaviorImpl impl)
            {
                impl.greeting = null;
            }

            protected override void OnBeforeDestroy() { }
        }
    }
}
