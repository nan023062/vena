// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly.Tests.BehaviorRuntime
{

    /// <summary>
    /// BehaviorResult 双态返回值 smoke 用例 —— 装配 Selector / Sequence / Parallel 的最小图，
    /// 验证双态语义（Running / Done）在父子节点间正确推进；
    /// 另验证异常路径（基类 try/catch + Logger.Error + Done 收尾，不承载于返回值）。
    ///
    /// 运行约定：
    ///   var smoke = new BehaviorResultSmoke(host);
    ///   smoke.RunSequence();    // 期望：两个 SampleBehaviorImpl1 节点串行；图在第二帧之内进入 !playing。
    ///   smoke.RunParallel();    // 期望：两个 SampleBehaviorImpl1 节点并行；同帧完成；图返回 !playing。
    ///   smoke.RunSelector();    // 期望：所有条件求 false → _selected == null → OnTick 立即 Done。
    ///   smoke.RunExceptionPath(captureHost);  // 期望：叶子 Tick 抛出 → Logger.Error 被调用 + 图正常推进到 !playing（不崩溃）。
    ///
    /// 由于本模块不引用任何测试框架，本类作为可调用脚手架；接入 NUnit/XUnit 时直接当 [Test] body 用。
    /// </summary>
    public sealed class BehaviorResultSmoke
    {
        private readonly IBlocklyHost _host;

        public BehaviorResultSmoke(IBlocklyHost host)
        {
            _host = host;
        }

        public bool RunSequence()
        {
            var rootSrc = new BehaviorGraph
            {
                root = new SequenceNode
                {
                    sources = new BehaviorNodeSource[]
                    {
                        new SampleBehaviorImpl1Source(),
                        new SampleBehaviorImpl1Source(),
                    },
                },
            };

            var graph = new BehaviorGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(rootSrc);
            graph.Start(subject: null, host: _host);

            // SampleBehaviorImpl1.Tick 当前返回 Done → Sequence 应该在第一帧就把第二个跑掉，
            // 所以 Update 第二帧应已退出 playing。
            graph.Update(0.016f);
            graph.Update(0.016f);

            bool ok = !graph.playing;
            graph.Destroy();
            return ok;
        }

        public bool RunParallel()
        {
            var rootSrc = new BehaviorGraph
            {
                root = new ParallelNode
                {
                    sources = new BehaviorNodeSource[]
                    {
                        new SampleBehaviorImpl1Source(),
                        new SampleBehaviorImpl1Source(),
                    },
                },
            };

            var graph = new BehaviorGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(rootSrc);
            graph.Start(subject: null, host: _host);

            // 两个 SampleBehaviorImpl1 同帧 Tick 都 Done → ParallelNode.OnTick 返回 Done。
            graph.Update(0.016f);

            bool ok = !graph.playing;
            graph.Destroy();
            return ok;
        }

        public bool RunSelector()
        {
            // 0 条件 + 0 行为 → Selector.Initialize 不创建任何分支 → _selected == null → OnTick 立即 Done
            var rootSrc = new BehaviorGraph
            {
                root = new SelectorNode
                {
                    conditions = new LogicGraph[0],
                    behaviors = new BehaviorNodeSource[0],
                },
            };

            var graph = new BehaviorGraph.Blockly();
            graph.Set(subject: null, host: _host);
            graph.SetSource(rootSrc);
            graph.Start(subject: null, host: _host);

            graph.Update(0.016f);

            bool ok = !graph.playing;
            graph.Destroy();
            return ok;
        }

        /// <summary>
        /// 异常路径用例：
        ///   叶子 Tick 抛出异常 → 基类 try/catch 捕获 → Logger.Error 记录 → 返回 BehaviorResult.Done →
        ///   图视作正常完成、推进到 !playing；调用方不应观察到异常逃逸。
        ///
        /// 调用方需传入一个能观察 Error 调用次数的 Host 实现（例如内嵌 ErrorCountingLogger 的 stub host）；
        /// 这里返回 (recovered, errorOccurred) 由调用方自行断言。
        /// </summary>
        public (bool recovered, bool errorOccurred) RunExceptionPath(IBlocklyHost throwingHost, System.Func<bool> errorObserved)
        {
            var rootSrc = new BehaviorGraph
            {
                root = new ThrowingBehaviorImplSource(),
            };

            var graph = new BehaviorGraph.Blockly();
            graph.Set(subject: null, host: throwingHost);
            graph.SetSource(rootSrc);
            graph.Start(subject: null, host: throwingHost);

            // 叶子 Tick 抛异常 → 基类 catch → Logger.Error + Done → 图本帧完事退出 playing。
            graph.Update(0.016f);

            bool recovered = !graph.playing;       // 异常不应让图卡住
            bool errorOccurred = errorObserved();  // 异常应通过 logger 旁路报告
            graph.Destroy();
            return (recovered, errorOccurred);
        }
    }

    /// <summary>抛出异常的叶子节点 IBehavior —— 用于 RunExceptionPath。</summary>
    public sealed class ThrowingBehaviorImpl : IBehavior
    {
        public void Start(BehaviorGraph.Blockly graph) { }

        public BehaviorResult Tick(BehaviorGraph.Blockly graph, float deltaTime)
        {
            throw new System.InvalidOperationException("smoke: deliberate Tick exception");
        }

        public void LateTick(BehaviorGraph.Blockly graph, float deltaTime) { }

        public void Finish(BehaviorGraph.Blockly graph) { }
    }

    [BlocklySource("测试行为/异常行为", typeof(ThrowingBehaviorImplSource.Node))]
    public sealed class ThrowingBehaviorImplSource : BehaviorNodeSource<ThrowingBehaviorImpl>
    {
        sealed class Node : BehaviorNode<ThrowingBehaviorImplSource, ThrowingBehaviorImpl>
        {
            protected override void Initialize() { }
            protected override void InitializeProperties(ThrowingBehaviorImpl behaviorImpl) { }
            protected override void CleanProperties(ThrowingBehaviorImpl behaviorImpl) { }
            protected override void OnBeforeDestroy() { }
        }
    }
}
