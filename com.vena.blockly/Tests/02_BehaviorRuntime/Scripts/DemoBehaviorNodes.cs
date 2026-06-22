// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Blockly.Tests.BehaviorRuntime
{
    // -------------------------------------------------------------------------
    // Demo-local LogicGraph const bool source
    //
    // 用于 BehaviorBranchDemo：BranchNode.condition 字段是 LogicGraph，
    // 喂一个能 Call<bool>() 的常量节点即可让 if-else 选边。
    // -------------------------------------------------------------------------

    public sealed class DemoConstBoolImpl : IFunctionImpl<bool>
    {
        public bool value;

        public bool Evaluate() => value;
    }

    [BlocklySource("Demo常量/Bool", typeof(DemoConstBoolSource.Node))]
    public sealed class DemoConstBoolSource : Function<DemoConstBoolImpl, bool>
    {
        [BlocklySourceSlot("常量", 1)]
        public bool value;

        sealed class Node : Block<DemoConstBoolSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(DemoConstBoolImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(DemoConstBoolImpl impl) { }
        }
    }

    // -------------------------------------------------------------------------
    // Demo-local LogicGraph const int source
    //
    // 用于 BehaviorLoopDemo：LoopNode.loopCount 字段是 LogicGraph<int>，
    // 喂一个能 Call<int>() 的常量节点定循环次数。
    // -------------------------------------------------------------------------

    public sealed class DemoConstIntImpl : IFunctionImpl<int>
    {
        public int value;

        public int Evaluate() => value;
    }

    [BlocklySource("Demo常量/Int", typeof(DemoConstIntSource.Node))]
    public sealed class DemoConstIntSource : Function<DemoConstIntImpl, int>
    {
        [BlocklySourceSlot("常量", 1)]
        public int value;

        sealed class Node : Block<DemoConstIntSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(DemoConstIntImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(DemoConstIntImpl impl) { }
        }
    }

    // -------------------------------------------------------------------------
    // Demo-local LogicGraph const string source
    //
    // 喂 HelloBehaviorSource.greeting（LogicGraph）：codegen 的 Node.InitializeProperties
    // 调 _greeting.Call<string>()，该常量节点 Evaluate 时 Push 自身 value。
    // -------------------------------------------------------------------------

    public sealed class DemoConstStringImpl : IFunctionImpl<string>
    {
        public string value;

        public string Evaluate() => value;
    }

    [BlocklySource("Demo常量/String", typeof(DemoConstStringSource.Node))]
    public sealed class DemoConstStringSource : Function<DemoConstStringImpl, string>
    {
        [BlocklySourceSlot("常量", 1)]
        public string value;

        sealed class Node : Block<DemoConstStringSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(DemoConstStringImpl impl) { impl.value = source.value; }
            protected override void CleanProperties(DemoConstStringImpl impl) { impl.value = null; }
        }
    }

    // -------------------------------------------------------------------------
    // 多帧 Running 叶子：CountdownBehavior
    //
    // Start 时 _remaining = ticksToRun；Tick 每帧 _remaining--；
    // 仍 > 0 → Running，归零 → Done。用于 BehaviorParallelMultiTickDemo
    // 演示 Parallel 同时跑多个不同步长的 Running 叶子。
    // -------------------------------------------------------------------------

    public sealed class CountdownBehaviorImpl : IBehavior
    {
        public int ticksToRun;
        public string label;

        private int _remaining;

        public void Start(BehaviorGraph.Blockly graph)
        {
            _remaining = ticksToRun;
            Debug.Log($"[Countdown:{label}] Start, remaining={_remaining}");
        }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            _remaining--;
            Debug.Log($"[Countdown:{label}] Tick, remaining={_remaining}");
            return _remaining > 0 ? BehaviorResult.Running : BehaviorResult.Done;
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime) { }

        public void Finish(BehaviorGraph.Blockly graph)
        {
            Debug.Log($"[Countdown:{label}] Finish");
        }
    }

    [BlocklySource("示例行为/Countdown", typeof(CountdownBehaviorSource.Node))]
    public sealed class CountdownBehaviorSource : BehaviorNodeSource<CountdownBehaviorImpl>
    {
        public int ticksToRun = 1;
        public string label;

        sealed class Node : BehaviorNode<CountdownBehaviorSource, CountdownBehaviorImpl>
        {
            protected override void Initialize() { }

            protected override void InitializeProperties(CountdownBehaviorImpl impl)
            {
                impl.ticksToRun = source.ticksToRun;
                impl.label = source.label;
            }

            protected override void CleanProperties(CountdownBehaviorImpl impl)
            {
                impl.label = null;
            }

            protected override void OnBeforeDestroy() { }
        }
    }

    // -------------------------------------------------------------------------
    // Procedure 副作用源：LogSignal
    //
    // 用于 TimelineSignalDemo：LogicClip 的 onBegin/onFrame/onEnd 以及
    // Signal.source 都是 LogicGraph（无返回值的 Procedure 形态）。
    // 这个源在 Evaluate 时直接 Debug.Log message，作为可观察的副作用。
    // -------------------------------------------------------------------------

    public sealed class LogSignalImpl : IProcedureImpl
    {
        public string message;

        public void Evaluate()
        {
            Debug.Log($"[Signal] {message}");
        }
    }

    [BlocklySource("Demo信号/Log", typeof(LogSignalSource.Node))]
    public sealed class LogSignalSource : Procedure<LogSignalImpl>
    {
        [BlocklySourceSlot("消息", 1)]
        public string message;

        sealed class Node : Block<LogSignalSource>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(LogSignalImpl impl) { impl.message = source.message; }
            protected override void CleanProperties(LogSignalImpl impl) { impl.message = null; }
        }
    }
}
